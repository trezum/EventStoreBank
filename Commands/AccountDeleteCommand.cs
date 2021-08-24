using Events;
using Model;
using System.Threading;
using System.Threading.Tasks;

namespace Commands
{
    public class AccountDeleteCommand : CommandBase<AccountDeletedEvent>
    {
        public AccountDeleteCommand(BankContext context) : base(context) { }
        public override async Task DBChanges(AccountDeletedEvent model, CancellationToken cancellationToken)
        {
            var account = await _context.Accounts.FindAsync(new object[] { model.AggregateId }, cancellationToken: cancellationToken);
            _context.Remove(account);
        }
    }
}
