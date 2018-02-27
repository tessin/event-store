using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EventCore.SqlServer
{
    class SqlEventStoreWithBulkCopyAppend : DelegateEventStore, ISqlEventStore
    {
        public SqlEventStoreDatabase Database { get; }

        public SqlEventStoreWithBulkCopyAppend(ISqlEventStore eventStore)
            : base(eventStore)
        {
            Database = eventStore.Database;
        }

        public override async Task AppendAsync(IEnumerable<UncommittedEvent> uncommitted)
        {
            using (var conn = await Database.OpenAsync())
            {
                var batch = new SqlBulkCopy(conn);

                batch.DestinationTableName = "es.EventStore";

                batch.ColumnMappings.Add(0, "StreamId");
                batch.ColumnMappings.Add(1, "SequenceNumber");
                batch.ColumnMappings.Add(2, "TypeId");
                batch.ColumnMappings.Add(3, "Payload");
                batch.ColumnMappings.Add(4, "UncompressedSize");
                batch.ColumnMappings.Add(5, "Created");

                batch.EnableStreaming = true;

                try
                {
                    using (var reader = new SqlEventStoreWithBulkCopy(uncommitted))
                    {
                        await batch.WriteToServerAsync(reader);
                    }
                }
                catch (SqlException ex) when (ex.Number == 2601)
                {
                    throw new EventRaceException("a data race has been detected", ex);
                }
            }
        }
    }
}
