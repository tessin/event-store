using EventSourcing;
using EventSourcing.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace SqlEventStoreTests
{
    [TestClass]
    public abstract class EventStoreBaseTest
    {
        private SqlLocalDB _db;
        public SqlEventStore EventStore { get; private set; }

        [TestInitialize]
        public void Initialize()
        {
            _db = new SqlLocalDB();

            EventStore = new SqlEventStore(_db.ConnectionString);
            EventStore.Database.InitializeAsync().GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var e in EventStore.GetEnumerable())
            {
                Debug.WriteLine($"{e.Id},{e.StreamId},{e.SequenceNumber},{e.TypeId},{e.Payload},{e.UncompressedSize},{e.Created}");
            }

            EventStore = null;

            _db.Dispose();
            _db = null;
        }

        protected static byte[] Payload(int size)
        {
            var bytes = new byte[size];
            for (int i = 0; i < size; i++)
            {
                bytes[i] = (byte)i;
            }
            return bytes;
        }

        protected class EventAssertionBuilder
        {
            public Event Actual { get; }

            public EventAssertionBuilder(Event actual)
            {
                if (actual == null)
                {
                    throw new ArgumentNullException(nameof(actual));
                }
                this.Actual = actual;
            }

            public EventAssertionBuilder Id(long expected)
            {
                Assert.AreEqual(expected, Actual.Id);
                return this;
            }

            public EventAssertionBuilder StreamId(Guid expected)
            {
                Assert.AreEqual(expected, Actual.StreamId);
                return this;
            }

            public EventAssertionBuilder SequenceNumber(int expected)
            {
                Assert.AreEqual(expected, Actual.SequenceNumber);
                return this;
            }

            public EventAssertionBuilder TypeId(Guid expected)
            {
                Assert.AreEqual(expected, Actual.TypeId);
                return this;
            }

            public EventAssertionBuilder Payload(byte[] expected)
            {
                CollectionAssert.AreEqual(expected, Actual.Payload);
                return this;
            }

            public EventAssertionBuilder UncompressedSize(int expected)
            {
                Assert.AreEqual(expected, Actual.UncompressedSize);
                return this;
            }

            public EventAssertionBuilder Created(DateTimeOffset expected)
            {
                Assert.AreEqual(expected.Ticks / TimeSpan.TicksPerSecond, Actual.Created.Ticks / TimeSpan.TicksPerSecond, 1);
                return this;
            }
        }

        protected static EventAssertionBuilder AssertEvent(Event actual)
        {
            return new EventAssertionBuilder(actual);
        }
    }
}
