using EventCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlEventStoreTests
{
    public static class AssertHelper
    {
        public struct EventAssertion
        {
            private readonly Event Actual;

            public EventAssertion(Event actual)
            {
                this.Actual = actual;
            }

            public void IsEqualTo(
                long? id = null,
                Guid? streamId = null,
                int? sequenceNumber = null,
                Guid? typeId = null,
                byte[] payload = null,
                int? uncompressedSize = null,
                DateTimeOffset? created = null
                )
            {
                if (id.HasValue)
                {
                    Assert.AreEqual(id.Value, Actual.Id);
                }

                if (streamId.HasValue)
                {
                    Assert.AreEqual(streamId.Value, Actual.StreamId);
                }

                if (sequenceNumber.HasValue)
                {
                    Assert.AreEqual(sequenceNumber.Value, Actual.SequenceNumber);
                }

                if (typeId.HasValue)
                {
                    Assert.AreEqual(typeId.Value, Actual.TypeId);
                }

                if (payload != null)
                {
                    CollectionAssert.AreEqual(payload, Actual.Payload);
                }

                if (uncompressedSize.HasValue)
                {
                    Assert.AreEqual(uncompressedSize.Value, Actual.UncompressedSize);
                }

                if (created.HasValue)
                {
                    Assert.AreEqual(created.Value.Ticks / TimeSpan.TicksPerSecond, Actual.Created.Ticks / TimeSpan.TicksPerSecond, 1);
                }
            }
        }

        public static EventAssertion Event(this Assert assert, Event actual)
        {
            return new EventAssertion(actual);
        }
    }
}
