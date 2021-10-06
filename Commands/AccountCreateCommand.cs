using Events;
using Model;
using System.Threading;
using System.Threading.Tasks;

namespace Commands
{
    public class AccountCreateCommand : CommandBase<AccountCreatedEvent>
    {
        public AccountCreateCommand(BankContext context) : base(context) { }
        public override async Task DBChanges(AccountCreatedEvent accountCreatedEvent, CancellationToken cancellationToken)
        {
            await _context.Accounts.AddAsync(new Account()
            {
                Balance = 0,
                Id = accountCreatedEvent.AggregateId,
                OwnerName = accountCreatedEvent.OwnerName
            }, cancellationToken);
        }
    }
}
