using System;

namespace Events
{
    public class AmountDepositedEvent : EventBase
    {
        public decimal Amount { get; set; }
        // If this property is null the source is cash otherwise it is from some other account.
        public Guid? Source { get; set; }
    }
}
