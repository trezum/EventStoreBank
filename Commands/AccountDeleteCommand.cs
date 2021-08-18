using Events;
using Model;
using System.Threading.Tasks;

namespace Commands
{
    public class AccountDeleteCommand
    {
        private readonly BankContext _context;

        public AccountDeleteCommand(BankContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(AccountDeleted accountDeletedEvent)
        {
            var account = await _context.Accounts.FindAsync(accountDeletedEvent.AggregateId);
            _context.Remove(account);
            await _context.SaveChangesAsync();
        }
    }
}
