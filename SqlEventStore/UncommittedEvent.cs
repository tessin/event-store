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
        public byte[] Payload { get; }
        public int UncompressedSize { get; }
        public DateTimeOffset Created { get; }

        public UncommittedEvent(Guid streamId, int sequenceNumber, Guid typeId, byte[] payload, int uncompressedSize, DateTimeOffset created)
        {
            this.StreamId = streamId;
            this.SequenceNumber = sequenceNumber;
            this.TypeId = typeId;
            this.Payload = payload;
            this.UncompressedSize = uncompressedSize;
            this.Created = created;
        }
    }
}
