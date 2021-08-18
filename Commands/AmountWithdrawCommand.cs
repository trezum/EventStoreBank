using Events;
using Model;
using System.Threading.Tasks;

namespace Commands
{
    public class AmountWithdrawCommand
    {
        private readonly BankContext _context;

        public AmountWithdrawCommand(BankContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(AmountWithdrawn amountWithdrawnEvent)
        {
            var account = await _context.Accounts.FindAsync(amountWithdrawnEvent.AggregateId);
            account.Balance -= amountWithdrawnEvent.Amount;
            account.EventVersion = amountWithdrawnEvent.EventVersion;
            await _context.SaveChangesAsync();
        }
    }
}
