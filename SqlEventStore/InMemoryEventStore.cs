using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlEventStore
{
    public class InMemoryEventStore : IEventStore
    {
        private readonly List<Event> _store = new List<Event>();
        private readonly Dictionary<Guid, List<int>> _index = new Dictionary<Guid, List<int>>();

        public Task AppendAsync(IEnumerable<UncommittedEvent> uncommitted)
        {
            lock (this)
            {
                var store = _store;
                var index = _index;

                // check invariant

                foreach (var g in uncommitted.GroupBy(x => x.StreamId))
                {
                    int sequenceNumber = 0;
                    if (index.TryGetValue(g.Key, out var stream))
                    {
                        sequenceNumber = store[stream[stream.Count - 1]].SequenceNumber;
                    }
                    foreach (var e in g.OrderBy(x => x.SequenceNumber))
                    {
                        if (!(e.SequenceNumber == sequenceNumber + 1))
                        {
                            throw new EventStoreDataRaceException($"cannot commit StreamID={e.StreamId}, SequenceNumber={e.SequenceNumber}", null);
                        }
                        sequenceNumber++;
                    }
                }

                // commit

                foreach (var e in uncommitted)
                {
                    var id = _store.Count + 1;
                    if (index.TryGetValue(e.StreamId, out var stream))
                    {
                        stream.Add(id);
                    }
                    else
                    {
                        index.Add(e.StreamId, new List<int> { id });
                    }
                    store.Add(new Event(id, e.StreamId, e.SequenceNumber, e.TypeId, e.Payload, e.UncompressedSize, e.Created));
                }
            }

            return Task.FromResult(0);
        }

        public IEnumerable<Event> GetEnumerable(long minId = 1, long maxId = long.MaxValue)
        {
            if (minId < 1) throw new ArgumentOutOfRangeException(nameof(minId));

            var store = _store;
            if (store.Count == 0)
            {
                yield break;
            }

            var upperBound = (int)Math.Min(maxId, store.Count);
            for (int i = (int)(minId - 1); i <= upperBound; i++)
            {
                yield return store[i];
            }
        }
    }
}
