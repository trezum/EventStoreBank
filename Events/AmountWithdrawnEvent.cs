using System;

namespace Events
{
    public class AmountWithdrawnEvent : EventBase
    {
        public decimal Amount { get; set; }
        // If this property is null the destination is cash otherwise it is to some other account.
        public Guid? Destination { get; set; }
    }
}
