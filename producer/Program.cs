using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;

class UserActionProducer
{
    private static readonly string[] Actions = new[]
    {
        "Login", "Logout", "PageView", "Search",
        "Purchase", "AddToCart", "RemoveFromCart",
        "ProfileUpdate", "FollowUser", "UnfollowUser",
        "Like", "Unlike", "Comment", "DeleteComment",
        "SubscriptionStart", "SubscriptionCancel"
    };

    private static readonly Random Random = new();

    public static async Task Main(string[] args)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "kafka:9092"
        };

        string topic = "user-actions";

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        Console.WriteLine($"Producing user action events to '{topic}'...");

        while (true)
        {
            var userAction = new
            {
                UserId = Guid.NewGuid(),
                Action = Actions[Random.Next(Actions.Length)],
                Metadata = new
                {
                    IpAddress = $"192.168.1.{Random.Next(1, 255)}",
                    Device = Random.Next(2) == 0 ? "Chrome on Windows" : "Safari on iPhone"
                },
                Timestamp = DateTime.UtcNow
            };

            string messageValue = JsonSerializer.Serialize(userAction);

            var deliveryResult = await producer.ProduceAsync(
                topic,
                new Message<Null, string> { Value = messageValue });

            Console.WriteLine($"Sent: {deliveryResult.Value}");

            await Task.Delay(500);
        }
    }
}
