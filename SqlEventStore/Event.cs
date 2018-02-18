using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    public class Event
    {
        public long Id { get; }
        public Guid StreamId { get; }
        public int SequenceNumber { get; }
        public Guid TypeId { get; }
        public byte[] Payload { get; }
        public int UncompressedSize { get; }
        public DateTimeOffset Created { get; }

        public Event(long id, Guid streamId, int sequenceNumber, Guid typeId, byte[] payload, int uncompressedSize, DateTimeOffset created)
        {
            this.Id = id;
            this.StreamId = streamId;
            this.SequenceNumber = sequenceNumber;
            this.TypeId = typeId;
            this.Payload = payload;
            this.UncompressedSize = uncompressedSize;
            this.Created = created;
        }
    }
}
