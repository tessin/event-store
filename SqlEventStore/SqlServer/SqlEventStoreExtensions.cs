namespace EventCore.SqlServer
{
    public static class SqlEventStoreExtensions
    {
        public static ISqlEventStore WithBulkCopyAppend(this ISqlEventStore eventStore)
        {
            if (eventStore is SqlEventStoreWithBulkCopyAppend)
            {
                return eventStore;
            }
            return new SqlEventStoreWithBulkCopyAppend(eventStore);
        }
    }
}
