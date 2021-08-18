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
        public long EventVersion { get; set; }
    }
}
