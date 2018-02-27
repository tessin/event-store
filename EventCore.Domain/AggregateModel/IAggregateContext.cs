using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.AggregateModel
{
    public interface IAggregateContext
    {
        TAggregate Create<TAggregate>(Guid? aggregateId = null)
            where TAggregate : IAggregate, new();

        TAggregate Rehydrate<TAggregate>(Guid aggregateId)
            where TAggregate : IAggregate, new();

        void SaveChanges();
    }
}
