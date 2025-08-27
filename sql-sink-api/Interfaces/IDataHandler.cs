namespace sql_sink_api.Interfaces
{
    public interface IDataHandler
    {
        Task<IEnumerable<object>> Get(DateTime? startTimeStamp, DateTime? endTimeStamp);
    }
}
