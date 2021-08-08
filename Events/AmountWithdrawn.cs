namespace Events
{
    public class AmountWithdrawn : IEvent
    {
        public string AggregateId { get; set; }
        public decimal Amount { get; set; }

    }
}
