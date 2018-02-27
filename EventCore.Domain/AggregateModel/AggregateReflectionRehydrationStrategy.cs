using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.AggregateModel
{
    class AggregateReflectionRehydrationStrategy<TAggregate> : IAggregateRehydrationStrategy<TAggregate>
            where TAggregate : IAggregate
    {
        private readonly Dictionary<Guid, Action<TAggregate, Event>> _dispatcher = new Dictionary<Guid, Action<TAggregate, Event>>();

        public AggregateReflectionRehydrationStrategy()
        {
            var aggregateType = typeof(TAggregate);

            var eventHandlerCreateMethod = typeof(AggregateEventHandler).GetMethod(nameof(AggregateEventHandler.Create));

            foreach (var m in aggregateType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == nameof(IHandler<object>.Handle)))
            {
                var p0 = m.GetParameters()[0];
                var eventType = p0.ParameterType;
                var m2 = eventHandlerCreateMethod.MakeGenericMethod(aggregateType, eventType);
                var eventHandler = (AggregateEventHandler<TAggregate>)m2.Invoke(null, null);

                _dispatcher.Add(eventHandler.typeId, eventHandler.handler);
            }
        }

        public void Rehydrate(IEnumerable<Event> source, TAggregate aggregate)
        {
            foreach (var e in source)
            {
                _dispatcher[e.TypeId](aggregate, e);

                aggregate.Version++;
            }
        }
    }
}
