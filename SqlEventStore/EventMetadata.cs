using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace EventCore.EventModel
{
    public static class EventMetadata
    {
        struct Entry
        {
            public readonly Type Type;
            public readonly Func<Event, object> Envelope;

            public Entry(Type type, Func<Event, object> envelope)
            {
                this.Type = type;
                this.Envelope = envelope;
            }
        }

        private static Dictionary<Guid, Entry> _map = new Dictionary<Guid, Entry>();

        public static void RegisterType(Guid typeId, Type type, Func<Event, object> envelope)
        {
            InterlockedExtensions.CompareExchangeAdd(ref _map, typeId, new Entry(type, envelope));
        }

        public static Type Resolve(Guid typeId)
        {
            return _map[typeId].Type;
        }

        public static object Envelope(Guid typeId, Event e)
        {
            return _map[typeId].Envelope(e);
        }
    }

    public static class EventMetadata<TEvent>
        where TEvent : class, new()
    {
        public static readonly Guid TypeId;

        static EventMetadata()
        {
            var type = typeof(TEvent);
            var typeInfo = type.GetTypeInfo();

            var guidAttribute = typeInfo.GetCustomAttribute<GuidAttribute>();

            var typeId = new Guid(guidAttribute.Value);

            EventMetadata.RegisterType(typeId, type, (Event e) => new EventEnvelope<TEvent>(e.Id, e.StreamId, e.SequenceNumber, DeserializeObject<TEvent>(e.Payload), e.Created));

            TypeId = typeId;
        }

        public static EventEnvelope<TEvent> Envelope(Event e)
        {
            return (EventEnvelope<TEvent>)EventMetadata.Envelope(TypeId, e);
        }

        private static T DeserializeObject<T>(byte[] payload)
        {
            var value = Encoding.UTF8.GetString(payload);
            var obj = JsonConvert.DeserializeObject<T>(value);
            return obj;
        }
    }
}
