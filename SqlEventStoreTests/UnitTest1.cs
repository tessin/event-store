using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventSourcing;
using System.Linq;
using EventSourcing.SqlServer;

namespace SqlEventStoreTests
{
    [TestClass]
    public class UnitTest1 : EventStoreBaseTest
    {
        [TestMethod]
        public async Task Append_1_Test()
        {
            var streamId = Guid.NewGuid();
            var created = DateTimeOffset.Now;

            await EventStore.AppendAsync(new[] { new UncommittedEvent(streamId, 1, new Guid(), Payload(1), 1, created) });

            var e = EventStore.GetEnumerable().Single();

            Assert.That.Event(e).IsEqualTo(1, streamId, 1, new Guid(), Payload(1), 1, created);
        }

        [TestMethod]
        public async Task Append_1_BulkCopyTest()
        {
            var streamId = Guid.NewGuid();
            var created = DateTimeOffset.Now;

            await EventStore.WithBulkCopyAppend().AppendAsync(new[] { new UncommittedEvent(streamId, 1, new Guid(), Payload(1), 1, created) });

            var e = EventStore.GetEnumerable().Single();

            Assert.That.Event(e).IsEqualTo(1, streamId, 1, new Guid(), Payload(1), 1, created);
        }

        [TestMethod]
        public async Task Append_3_Test()
        {
            var streamId = Guid.NewGuid();
            var created = DateTimeOffset.Now;

            var es = new[] {
                new UncommittedEvent(streamId, 1, new Guid(), Payload(1), 1, created),
                new UncommittedEvent(streamId, 2, new Guid(), Payload(2), 2, created),
                new UncommittedEvent(streamId, 3, new Guid(), Payload(3), 3, created),
            };

            await EventStore.AppendAsync(es);

            var e = EventStore.GetEnumerable().ToList();

            Assert.That.Event(e[0]).IsEqualTo(1, streamId, 1, new Guid(), Payload(1), 1, created);
            Assert.That.Event(e[1]).IsEqualTo(2, streamId, 2, new Guid(), Payload(2), 2, created);
            Assert.That.Event(e[2]).IsEqualTo(3, streamId, 3, new Guid(), Payload(3), 3, created);
        }

        [TestMethod]
        public async Task Append_3_BulkCopyTest()
        {
            var streamId = Guid.NewGuid();
            var created = DateTimeOffset.Now;

            var es = new[] {
                new UncommittedEvent(streamId, 1, new Guid(), Payload(1), 1, created),
                new UncommittedEvent(streamId, 2, new Guid(), Payload(2), 2, created),
                new UncommittedEvent(streamId, 3, new Guid(), Payload(3), 3, created),
            };

            await EventStore.WithBulkCopyAppend().AppendAsync(es);

            var e = EventStore.GetEnumerable().ToList();

            Assert.That.Event(e[0]).IsEqualTo(1, streamId, 1, new Guid(), Payload(1), 1, created);
            Assert.That.Event(e[1]).IsEqualTo(2, streamId, 2, new Guid(), Payload(2), 2, created);
            Assert.That.Event(e[2]).IsEqualTo(3, streamId, 3, new Guid(), Payload(3), 3, created);
        }
    }
}
