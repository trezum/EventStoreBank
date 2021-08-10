using System;

namespace Model
{
    public class Account
    {
        public Guid Id { get; set; }
        public string OwnerName { get; set; }
        public decimal Balance { get; set; }
        public long EventVersion { get; set; }

        //ListofTransactions? for more challenges
    }
}
