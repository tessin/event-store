using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<UncommittedEvent> uncommitted);

        IEnumerable<Event> GetEnumerable(long minId = 1, long maxId = long.MaxValue);

        IEnumerable<Event> GetEnumerableStream(Guid streamId, int minSequenceNumber = 1, int maxSequenceNumber = int.MaxValue);
    }
}
