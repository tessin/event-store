namespace EventCore.SqlServer
{
    public interface ISqlEventStore : IEventStore
    {
        SqlEventStoreDatabase Database { get; }
    }
}
