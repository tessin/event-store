using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.AggregateModel
{
    public interface IAggregateRehydrationStrategy<TAggregate>
            where TAggregate : IAggregate
    {
        void Rehydrate(IEnumerable<Event> source, TAggregate aggregate);
    }
}
