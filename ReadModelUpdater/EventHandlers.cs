using Commands;
using Events;
using EventStore.Client;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ReadModelUpdater
{
    public class EventHandlers
    {
        private readonly Dictionary<Type, ICommandInterface> _eventCommandMap;

        public EventHandlers(
            AccountCreateCommand accountCreateCommand,
            AmountWithdrawCommand amountWithdrawCommand,
            AmountDepositCommand amountDepositCommand,
            AccountDeleteCommand accountDeleteCommand
            )
        {
            _eventCommandMap = new Dictionary<Type, ICommandInterface>()
            {
                { typeof(AccountCreatedEvent), accountCreateCommand },
                { typeof(AmountWithdrawnEvent), amountWithdrawCommand },
                { typeof(AmountDepositedEvent), amountDepositCommand},
                { typeof(AccountDeletedEvent), accountDeleteCommand },
            };
        }


        public async Task handleEvents(ResolvedEvent evnt, CancellationToken cancellationToken)
        {
            await handleGenericEvent<AccountCreatedEvent>(evnt, cancellationToken);
            await handleGenericEvent<AccountDeletedEvent>(evnt, cancellationToken);
            await handleGenericEvent<AmountDepositedEvent>(evnt, cancellationToken);
            await handleGenericEvent<AmountWithdrawnEvent>(evnt, cancellationToken);
        }

        private async Task handleGenericEvent<T>(ResolvedEvent evnt, CancellationToken cancellationToken)
        {
            if (evnt.Event.EventType == typeof(T).Name)
            {
                Console.WriteLine(evnt.Event.EventType);
                var eventObject = JsonSerializer.Deserialize<T>(evnt.Event.Data.Span);
                var obj = _eventCommandMap.GetValueOrDefault(typeof(T));
                if (obj == null)
                {
                    throw new Exception("Event Type not found!");
                }
                var cmd = (CommandBase<T>)obj;
                await cmd.ExecuteAsync(eventObject, evnt.Event.Position.CommitPosition, evnt.Event.Position.PreparePosition, cancellationToken);
            }
        }
    }
}
