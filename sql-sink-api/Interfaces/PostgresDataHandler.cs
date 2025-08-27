using Dapper;
using Npgsql;

namespace sql_sink_api.Interfaces
{
    public class PostgresDataHandler : IDataHandler
    {
        string connectionString = "Host=postgres;Port=5432;Database=MyDb;Username=myuser;Password=mypassword"; //eventually move to config file
        public PostgresDataHandler() { }
        public async Task<IEnumerable<object>> Get(DateTime? startTimeStamp, DateTime? endTimeStamp)
        {
            string connectionString = "Host=postgres;Port=5432;Database=MyDb;Username=myuser;Password=mypassword";

            await using var connection = new NpgsqlConnection(connectionString);

            // Base SQL
            var sql = @"
        SELECT userid AS UserId, 
               action AS Action, 
               ipaddress AS IpAddress, 
               device AS Device, 
               timestamp AS Timestamp
        FROM useractions
        /**where**/
        ORDER BY timestamp;
    ";

            // Build dynamic WHERE clause
            var conditions = new List<string>();
            if (startTimeStamp.HasValue) conditions.Add("timestamp >= @Start");
            if (endTimeStamp.HasValue) conditions.Add("timestamp <= @End");

            if (conditions.Count > 0)
            {
                sql = sql.Replace("/**where**/", "WHERE " + string.Join(" AND ", conditions));
            }
            else
            {
                sql = sql.Replace("/**where**/", ""); // no filters, get all
            }

            var userActions = await connection.QueryAsync(sql, new { Start = startTimeStamp, End = endTimeStamp });

            return userActions;
        }
    }
}
