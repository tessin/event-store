﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class EventRaceException : Exception
    {
        public EventRaceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}