using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore
{
    public class EventDataRaceException : Exception
    {
        public EventDataRaceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
