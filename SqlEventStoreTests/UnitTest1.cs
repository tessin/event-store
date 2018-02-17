using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlEventStore;

namespace SqlEventStoreTests
{
    [TestClass]
    public class UnitTest1 : EventStoreBaseTest
    {
        [TestMethod]
        public async Task AppendTest()
        {
            await EventStore.AppendAsync(new[] { new UncommittedEvent(new Guid(), 1, new Guid(), new byte[0], 0, DateTimeOffset.UtcNow) });
        }

        [TestMethod]
        public async Task AppendManyTest()
        {
            var es = new[] {
                new UncommittedEvent(new Guid(), 1, new Guid(), new byte[0], 0, DateTimeOffset.UtcNow),
                new UncommittedEvent(new Guid(), 2, new Guid(), new byte[0], 0, DateTimeOffset.UtcNow),
                new UncommittedEvent(new Guid(), 3, new Guid(), new byte[0], 0, DateTimeOffset.UtcNow),
            };

            await EventStore.AppendAsync(es);
        }

        [TestMethod, ExpectedException(typeof(EventStoreDataRaceException))]
        public async Task AppendDataRaceTest()
        {
            var es = new[] {
                new UncommittedEvent(new Guid(), 1, new Guid(), new byte[0], 0, DateTimeOffset.UtcNow),
                new UncommittedEvent(new Guid(), 1, new Guid(), new byte[0], 0, DateTimeOffset.UtcNow),
            };

            await EventStore.AppendAsync(es);
        }
    }
}
