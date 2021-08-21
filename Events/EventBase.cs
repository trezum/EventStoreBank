using System;

namespace Events
{
    public class EventBase
    {
        public EventBase()
        {
            Timestamp = DateTime.Now;
        }
        public DateTime Timestamp { get; }
        public Guid AggregateId { get; set; }

        //TODO:Maybe this should be the version from allstream, see notes in IQuery.cs
        public long EventVersion { get; set; }
    }
}
