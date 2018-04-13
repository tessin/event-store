using EventCore.EventModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore
{
    public static class AggregateExtensions
    {
        public static void Emit<TAggregate, TEvent>(this TAggregate aggregate, TEvent e)
            where TAggregate : IAggregate, IHandler<TEvent>
            where TEvent : class, new()
        {
            aggregate.Handle(e); // ensure that the event can be handled given current state

            var sequenceNumber = aggregate.Version + 1;

            aggregate.UncommittedEvents.Append(new UncommittedEvent(aggregate.Id, sequenceNumber, EventMetadata<TEvent>.TypeId, e, DateTimeOffset.UtcNow));

            aggregate.Version = sequenceNumber;
        }
    }
}
