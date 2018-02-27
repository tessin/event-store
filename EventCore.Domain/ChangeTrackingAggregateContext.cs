using EventCore.AggregateModel;
using EventCore.EventModel;
using System;
using System.Collections.Generic;

namespace EventCore
{
    public class ChangeTrackingAggregateContext : IAggregateContext
    {
        private readonly IServiceProvider _container;
        private readonly IEventStore _eventStore;

        public ChangeTrackingAggregateContext(IServiceProvider container, IEventStore eventStore)
        {
            _container = container;
            _eventStore = eventStore;
        }

        private readonly List<UncommittedEvent> _uncommittedEvents = new List<UncommittedEvent>();

        public TAggregate Create<TAggregate>(Guid? aggregateId = null)
            where TAggregate : IAggregate, new()
        {
            var aggregate = new TAggregate();

            aggregate.Id = aggregateId ?? Guid.NewGuid();
            aggregate.UncommittedEvents = new UncommittedEventBatch(_uncommittedEvents);

            return aggregate;
        }

        public TAggregate Rehydrate<TAggregate>(Guid aggregateId)
            where TAggregate : IAggregate, new()
        {
            var aggregate = Create<TAggregate>(aggregateId);

            var strategy = (IAggregateRehydrationStrategy<TAggregate>)_container.GetService(typeof(IAggregateRehydrationStrategy<TAggregate>));
            if (strategy == null)
            {
                throw new InvalidOperationException($"required service '{nameof(IAggregateRehydrationStrategy<TAggregate>)}' not found");
            }

            strategy.Rehydrate(_eventStore.GetEnumerableStream(aggregateId), aggregate);

            return aggregate;
        }

        public void SaveChanges()
        {
            var batch = _uncommittedEvents.ToArray();
            _eventStore.AppendAsync(batch).GetAwaiter().GetResult();
            _uncommittedEvents.Clear();
        }
    }
}
