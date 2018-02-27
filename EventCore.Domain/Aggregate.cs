using EventCore.EventModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore
{
    public interface IAggregate
    {
        Guid Id { get; set; }
        int Version { get; set; }
        IUncommittedEventBatch UncommittedEvents { get; set; }
    }

    public abstract class Aggregate<TModel> : IAggregate
        where TModel : class
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public IUncommittedEventBatch UncommittedEvents { get; set; }

        public TModel Model { get; set; }
    }
}
