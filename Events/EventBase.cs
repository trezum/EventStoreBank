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
        public string AggregateId { get; set; }
    }
}
