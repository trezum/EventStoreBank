using Events;
using Model;
using System.Threading;
using System.Threading.Tasks;

namespace Commands
{
    public class AmountDepositCommand : CommandBase<AmountDepositedEvent>
    {
        public AmountDepositCommand(BankContext context) : base(context) { }
        public override async Task DBChanges(AmountDepositedEvent model, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FindAsync(new object[] { model.AggregateId }, cancellationToken: cancellationToken);
            account.Balance += model.Amount;
        }
    }
}
