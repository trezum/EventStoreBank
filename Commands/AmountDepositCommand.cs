using Events;
using Model;
using System.Threading.Tasks;

namespace Commands
{
    public class AmountDepositCommand
    {
        private readonly BankContext _context;

        public AmountDepositCommand(BankContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(AmountDeposited amountDepositedEvent)
        {
            var account = await _context.Accounts.FindAsync(amountDepositedEvent.AggregateId);
            account.Balance += amountDepositedEvent.Amount;
            account.EventVersion = amountDepositedEvent.EventVersion;
            await _context.SaveChangesAsync();
        }
    }
}
