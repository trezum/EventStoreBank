namespace Events
{
    public class AmountTransferred : EventBase
    {
        public string DestinationId { get; set; }
        public decimal Amount { get; set; }
    }
}
