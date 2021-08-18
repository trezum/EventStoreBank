using Events;
using Model;
using System.Threading.Tasks;

namespace Commands
{
    public class AccountCreateCommand
    {
        private readonly BankContext _context;

        public AccountCreateCommand(BankContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(AccountCreated accountCreatedEvent)
        {
            await _context.Accounts.AddAsync(new Account()
            {
                Balance = 0,
                EventVersion = accountCreatedEvent.EventVersion,
                Id = accountCreatedEvent.AggregateId,
                OwnerName = accountCreatedEvent.OwnerName

            });
            await _context.SaveChangesAsync();
        }
    }
}
