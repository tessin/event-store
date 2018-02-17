using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlEventStore.SqlClient
{
    public class SqlEventStore : IEventStore
    {
        private readonly string _connectionString;

        public string DataSource => new SqlConnectionStringBuilder(_connectionString).DataSource;
        public string Database => new SqlConnectionStringBuilder(_connectionString).InitialCatalog;

        public SqlEventStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        public virtual async Task AppendAsync(IEnumerable<UncommittedEvent> uncommitted)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

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
                    using (var reader = new UncommittedEventReader(uncommitted))
                    {
                        await batch.WriteToServerAsync(reader);
                    }
                }
                catch (SqlException ex) when (ex.Number == 2601)
                {
                    throw new EventStoreDataRaceException("a data race has been detected", ex);
                }
            }
        }

        public virtual IEnumerable<Event> GetEnumerable(long minId = 1, long maxId = long.MaxValue)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var cmd = new SqlCommand("select * from es.EventStore where Id between @min and @max", conn);

                cmd.Parameters.AddWithValue("@min", minId);
                cmd.Parameters.AddWithValue("@max", maxId);

                using (var reader = new SqlEventDataReader(cmd.ExecuteReader(CommandBehavior.SequentialAccess)))
                {
                    while (reader.Read())
                    {
                        yield return reader.Event;
                    }
                }
            }
        }

        public virtual IEnumerable<Event> GetEnumerableStream(Guid streamId, int minSequenceNumber = 1, int maxSequenceNumber = int.MaxValue)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                var sql = @"
declare @tmp table (
	Id bigint not null primary key
)
;

insert into @tmp
select Id
from es.EventStore
where StreamId = @streamId and SequenceNumber between @min and @max
order by SequenceNumber
;

select es.* 
from es.EventStore es
where Id in (select Id from @tmp)
order by Id
;
";

                var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@streamId", streamId);
                cmd.Parameters.AddWithValue("@min", minSequenceNumber);
                cmd.Parameters.AddWithValue("@max", maxSequenceNumber);

                using (var reader = new SqlEventDataReader(cmd.ExecuteReader(CommandBehavior.SequentialAccess)))
                {
                    while (reader.Read())
                    {
                        yield return reader.Event;
                    }
                }
            }
        }
    }
}
