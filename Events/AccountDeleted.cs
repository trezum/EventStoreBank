namespace Events
{
    public class AccountDeleted : IEvent
    {
        public string AggregateId { get; set; }
    }
}
