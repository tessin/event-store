using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore
{
    public class UncommittedEvent
    {
        public Guid StreamId { get; }
        public int SequenceNumber { get; }
        public Guid TypeId { get; }
        public object Payload { get; }
        public DateTimeOffset Created { get; }

        public UncommittedEvent(Guid streamId, int sequenceNumber, Guid typeId, object payload, DateTimeOffset created)
        {
            this.StreamId = streamId;
            this.SequenceNumber = sequenceNumber;
            this.TypeId = typeId;
            this.Payload = payload;
            this.Created = created;
        }
    }
}
