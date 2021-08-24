using Commands;
using Events;
using EventStore.Client;
using Model;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ReadModelUpdater
{
    public class EventHandlers
    {
        private readonly BankContext _bankContext;

        public EventHandlers(BankContext bankContext)
        {
            _bankContext = bankContext;
        }
        public async Task handleIfAmountWithdrawn(ResolvedEvent evnt, CancellationToken cancellationToken)
        {
            if (evnt.Event.EventType == typeof(AmountWithdrawnEvent).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AmountWithdrawnEvent>(evnt.Event.Data.Span);
                var cmd = new AmountWithdrawCommand(_bankContext);
                await cmd.ExecuteAsync(eventObject, evnt.Event.Position.CommitPosition, evnt.Event.Position.PreparePosition, cancellationToken);
            }
        }

        public async Task handleIfAmountDeposited(ResolvedEvent evnt, CancellationToken cancellationToken)
        {
            if (evnt.Event.EventType == typeof(AmountDepositedEvent).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AmountDepositedEvent>(evnt.Event.Data.Span);
                var cmd = new AmountDepositCommand(_bankContext);
                await cmd.ExecuteAsync(eventObject, evnt.Event.Position.CommitPosition, evnt.Event.Position.PreparePosition, cancellationToken);
            }
        }

        public async Task handleIfAccountDeleted(ResolvedEvent evnt, CancellationToken cancellationToken)
        {
            if (evnt.Event.EventType == typeof(AccountDeletedEvent).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AccountDeletedEvent>(evnt.Event.Data.Span);
                var cmd = new AccountDeleteCommand(_bankContext);
                await cmd.ExecuteAsync(eventObject, evnt.Event.Position.CommitPosition, evnt.Event.Position.PreparePosition, cancellationToken);
            }
        }

        public async Task handleIfAccountCreated(ResolvedEvent evnt, CancellationToken cancellationToken)
        {
            if (evnt.Event.EventType == typeof(AccountCreatedEvent).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AccountCreatedEvent>(evnt.Event.Data.Span);
                var cmd = new AccountCreateCommand(_bankContext);
                await cmd.ExecuteAsync(eventObject, evnt.Event.Position.CommitPosition, evnt.Event.Position.PreparePosition, cancellationToken);
            }
        }
    }
}
