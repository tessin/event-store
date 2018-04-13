using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventCore.EventModel
{
    public interface IUncommittedEventBatch
    {
        void Append(UncommittedEvent e);
    }

    public class UncommittedEventBatch : IUncommittedEventBatch
    {
        private readonly List<UncommittedEvent> _batch;

        public UncommittedEventBatch(List<UncommittedEvent> batch)
        {
            if (batch == null)
            {
                throw new ArgumentNullException(nameof(batch));
            }
            _batch = batch;
        }

        public void Append(UncommittedEvent uncommitted)
        {
            if (uncommitted == null)
            {
                throw new ArgumentNullException(nameof(uncommitted));
            }
            _batch.Add(uncommitted);
        }
    }
}
