using Events;
using Model;
using System.Threading;
using System.Threading.Tasks;

namespace Commands
{
    public class AmountWithdrawCommand : CommandBase<AmountWithdrawnEvent>
    {
        public AmountWithdrawCommand(BankContext context) : base(context) { }

        public override async Task DBChanges(AmountWithdrawnEvent model, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FindAsync(new object[] { model.AggregateId }, cancellationToken: cancellationToken);
            account.Balance -= model.Amount;
            account.EventVersion = model.EventVersion;
        }
    }
}
