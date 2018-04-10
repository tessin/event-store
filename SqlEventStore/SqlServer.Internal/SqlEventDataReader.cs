using System;
using System.Data.SqlClient;

namespace EventCore.SqlServer
{
    public class SqlEventDataReader : IDisposable
    {
        private readonly SqlDataReader _reader;

        public Event Event { get; private set; }

        public SqlEventDataReader(SqlDataReader reader)
        {
            _reader = reader;
        }

        public bool Read()
        {
            // todo: https://docs.microsoft.com/en-us/dotnet/api/system.data.sqlclient.sqldatareader.getstream?view=netframework-4.7.1

            // use GetStream and block compression to stream decompression (reusable private memory)

            // minimize GC pressure

            var reader = _reader;
            if (reader.Read())
            {
                var id = (long)reader.GetSqlInt64(0);
                var streamId = (Guid)reader.GetSqlGuid(1);
                var sequenceNumber = (int)reader.GetSqlInt32(2);
                var typeId = (Guid)reader.GetSqlGuid(3);
                var payload = reader.GetSqlBytes(4);
                var uncompressedSize = (int)reader.GetSqlInt32(5);
                var created = reader.GetDateTimeOffset(6);

                Event = new Event(
                    id,
                    streamId,
                    sequenceNumber,
                    typeId,
                    payload.Buffer,
                    uncompressedSize,
                    created
                );
                return true;
            }
            Event = null;
            return false;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
