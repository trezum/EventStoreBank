namespace Events
{
    public class AccountCreatedEvent : EventBase
    {
        public string OwnerName { get; set; }
    }
}
