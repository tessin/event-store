namespace EventSourcing.SqlServer
{
    public interface ISqlEventStore : IEventStore
    {
        SqlEventStoreDatabase Database { get; }
    }
}
