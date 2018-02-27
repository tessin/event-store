using EventCore.EventModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.AggregateModel
{
    class AggregateEventHandler<TAggregate>
    {
        public readonly Guid typeId;
        public readonly Action<TAggregate, Event> handler;

        public AggregateEventHandler(Guid typeId, Action<TAggregate, Event> handler)
        {
            this.typeId = typeId;
            this.handler = handler;
        }
    }

    class AggregateEventHandler
    {
        public static AggregateEventHandler<TAggregate> Create<TAggregate, TEvent>()
             where TAggregate : IHandler<TEvent>
        {
            return new AggregateEventHandler<TAggregate>(EventMetadata<TEvent>.TypeId, (aggregate, e) => aggregate.Handle(JsonConvert.DeserializeObject<TEvent>(e.Payload)));
        }
    }
}
