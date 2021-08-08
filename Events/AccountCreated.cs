namespace Events
{
    public class AccountCreated : IEvent
    {
        public string AggregateId { get; set; }
        public string OwnerName { get; set; }
    }
}
