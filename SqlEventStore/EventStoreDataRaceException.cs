using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlEventStore
{
    public class SqlEventStoreDataRaceException : Exception
    {
        public SqlEventStoreDataRaceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
