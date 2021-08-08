namespace Events
{
    public interface IEvent
    {
        public string AggregateId { get; set; }
    }
}
