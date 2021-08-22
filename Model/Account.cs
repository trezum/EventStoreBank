using System;

namespace Model
{
    public class Account
    {
        public Guid Id { get; set; }
        public string OwnerName { get; set; }
        public decimal Balance { get; set; }

        //This event version is not used in the Optimistic Concurrency Logic.
        public long EventVersion { get; set; }
    }
}
