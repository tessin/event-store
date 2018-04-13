using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading.Tasks;

namespace EventCore.SqlServer
{
    public class SqlEventStore : ISqlEventStore
    {
        public SqlEventStoreDatabase Database { get; }

        public SqlEventStore(string connectionString)
        {
            Database = new SqlEventStoreDatabase(connectionString);
        }

        public async Task AppendAsync(IEnumerable<UncommittedEvent> uncommitted)
        {
            var v = new List<SqlDataRecord>();

            foreach (var e in uncommitted)
            {
                var r = new SqlUncommittedEventDataRecord();

                r.StreamId = e.StreamId;
                r.SequenceNumber = e.SequenceNumber;
                r.TypeId = e.TypeId;
                r.Payload = new SqlBytes(e.Payload);
                r.UncompressedSize = e.UncompressedSize;
                r.Created = e.Created;

                v.Add(r.GetDataRecord());
            }

            using (var conn = await Database.OpenAsync())
            {
                var cmd = new SqlCommand("es.[Append]", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                var p = cmd.Parameters.AddWithValue("@uncommitted", v);
                p.SqlDbType = SqlDbType.Structured;
                p.TypeName = "es.UncommittedEvent";

                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (SqlException ex) when (ex.Number == 2601)
                {
                    throw new EventDataRaceException("a data race has been detected", ex);
                }
            }
        }

        public IEnumerable<Event> GetEnumerable(long minEventId = 1, long maxEventId = long.MaxValue)
        {
            using (var conn = Database.Open())
            {
                using (var cmd = new SqlCommand("es.GetEnumerable", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@min", minEventId);
                    cmd.Parameters.AddWithValue("@max", maxEventId);

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

        public IEnumerable<Event> GetEnumerableStream(Guid streamId, int minSequenceNumber = 1, int maxSequenceNumber = int.MaxValue)
        {
            using (var conn = Database.Open())
            {
                using (var cmd = new SqlCommand("es.GetEnumerableStream", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

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
}
