namespace Events
{
    public class AmountTransferred : IEvent
    {
        public string AggregateId { get; set; }
        public string DestinationId { get; set; }
        public decimal Amount { get; set; }
    }
}
