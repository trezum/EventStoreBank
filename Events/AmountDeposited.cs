namespace Events
{
    public class AmountDeposited : IEvent
    {
        public string AggregateId { get; set; }
        public decimal Amount { get; set; }
    }
}
