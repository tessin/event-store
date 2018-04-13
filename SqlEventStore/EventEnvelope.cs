using System;

namespace EventCore
{
    public class EventEnvelope<TPayload>
        where TPayload : class, new()
    {
        public long Id { get; }
        public Guid StreamId { get; }
        public int SequenceNumber { get; }
        public TPayload Payload { get; }
        public DateTimeOffset Created { get; }

        public EventEnvelope(long id, Guid streamId, int sequenceNumber, TPayload payload, DateTimeOffset created)
        {
            Id = id;
            StreamId = streamId;
            SequenceNumber = sequenceNumber;
            Payload = payload;
            Created = created;
        }
    }
}
