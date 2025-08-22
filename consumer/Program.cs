using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Dapper;
using Npgsql;

class UserActionConsumer
{
    public static async Task Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "user-action-loader",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        string topic = "user-actions";
        string connectionString = "Host=postgres;Port=5432;Database=MyDb;Username=myuser;Password=mypassword";

        // Ensure table exists
        await EnsureTableExists(connectionString);

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(topic);

        Console.WriteLine($"Listening for user actions on '{topic}'...");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                var cr = consumer.Consume(cts.Token);
                Console.WriteLine($"Received: {cr.Message.Value}");

                try
                {
                    var userAction = JsonSerializer.Deserialize<UserAction>(cr.Message.Value);

                    await using var connection = new NpgsqlConnection(connectionString);
                    await connection.ExecuteAsync(@"
                        INSERT INTO useractions (userid, action, ipaddress, device, timestamp)
                        VALUES (@userid, @action, @ipaddress, @device, @timestamp)",
                        new
                        {
                            userid = userAction.UserId,
                            action = userAction.Action,
                            ipaddress = userAction.Metadata.IpAddress,
                            device = userAction.Metadata.Device,
                            timestamp = userAction.Timestamp
                        });

                    consumer.Commit(cr);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process message: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Shutting down gracefully...");
        }
        finally
        {
            consumer.Close();
        }
    }

    private static async Task EnsureTableExists(string connectionString)
    {
        const int maxRetries = 10;
        const int delayMs = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS useractions (
                    id SERIAL PRIMARY KEY,
                    userid UUID NOT NULL,
                    action TEXT NOT NULL,
                    ipaddress TEXT,
                    device TEXT,
                    timestamp TIMESTAMPTZ NOT NULL
                );");

                Console.WriteLine("Ensured 'useractions' table exists.");
                return;
            }
            catch (PostgresException ex) when (ex.SqlState == "57P03")
            {
                Console.WriteLine($"Postgres not ready (attempt {attempt}/{maxRetries}), retrying in {delayMs}ms...");
                await Task.Delay(delayMs);
            }
        }

        throw new Exception("Could not connect to Postgres after multiple attempts.");
    }


    public class UserAction
    {
        public Guid UserId { get; set; }
        public string Action { get; set; }
        public Metadata Metadata { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class Metadata
    {
        public string IpAddress { get; set; }
        public string Device { get; set; }
    }
}
