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

        // TODO: Maybe this should be the version from allstream.
        // Include event version in read in order to reduce eventual concistency issues
        // When querying for updated data, the desired all eventVersion from the current write, could be sent to/with the query so it can wait for data to be updated.
        // https://youtu.be/FKFu78ZEIi8?t=1771
        public long EventVersion { get; set; }
    }
}
