using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlEventStore
{
    public interface IEventStore
    {
        Task AppendAsync(IEnumerable<UncommittedEvent> uncommitted);

        IEnumerable<SqlEvent> GetEnumerable(long minId = 1, long maxId = long.MaxValue);
    }
}
