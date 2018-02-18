using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing
{
    public abstract class DelegateEventStore : IEventStore
    {
        private readonly IEventStore _eventStore;

        protected DelegateEventStore(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public virtual Task AppendAsync(IEnumerable<UncommittedEvent> uncommitted)
        {
            return _eventStore.AppendAsync(uncommitted);
        }

        public virtual IEnumerable<Event> GetEnumerable(long minId = 1, long maxId = long.MaxValue)
        {
            return _eventStore.GetEnumerable(minId, maxId);
        }

        public virtual IEnumerable<Event> GetEnumerableStream(Guid streamId, int minSequenceNumber = 1, int maxSequenceNumber = int.MaxValue)
        {
            return _eventStore.GetEnumerableStream(streamId, minSequenceNumber, maxSequenceNumber);
        }
    }
}
