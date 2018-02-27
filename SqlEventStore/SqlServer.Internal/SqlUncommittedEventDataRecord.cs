using Microsoft.SqlServer.Server;
using System;
using System.Data;
using System.Data.SqlTypes;

namespace EventCore.SqlServer
{
    public class SqlUncommittedEventDataRecord
    {
        private static readonly SqlMetaData[] Schema = new SqlMetaData[]
        {
            new SqlMetaData("StreamId", SqlDbType.UniqueIdentifier),
            new SqlMetaData("SequenceNumber", SqlDbType.Int),
            new SqlMetaData("TypeId", SqlDbType.UniqueIdentifier),
            new SqlMetaData("Payload", SqlDbType.VarBinary, -1),
            new SqlMetaData("UncompressedSize", SqlDbType.Int),
            new SqlMetaData("Created", SqlDbType.DateTimeOffset),
        };

        private readonly SqlDataRecord _record;

        public SqlGuid StreamId { get { return _record.GetSqlGuid(0); } set { _record.SetSqlGuid(0, value); } }
        public SqlInt32 SequenceNumber { get { return _record.GetSqlInt32(1); } set { _record.SetSqlInt32(1, value); } }
        public SqlGuid TypeId { get { return _record.GetSqlGuid(2); } set { _record.SetSqlGuid(2, value); } }
        public SqlBytes Payload { get { return _record.GetSqlBytes(3); } set { _record.SetSqlBytes(3, value); } }
        public SqlInt32 UncompressedSize { get { return _record.GetSqlInt32(4); } set { _record.SetSqlInt32(4, value); } }
        public DateTimeOffset Created { get { return _record.GetDateTimeOffset(5); } set { _record.SetDateTimeOffset(5, value); } }

        public SqlUncommittedEventDataRecord()
        {
            this._record = new SqlDataRecord(Schema);
        }

        public SqlDataRecord GetDataRecord()
        {
            return _record;
        }
    }
}
