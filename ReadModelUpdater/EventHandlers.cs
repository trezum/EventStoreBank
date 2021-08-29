using Commands;
using Events;
using EventStore.Client;
using Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ReadModelUpdater
{
    public class EventHandlers
    {
        private readonly BankContext _bankContext;

        private readonly Dictionary<Type, ICommandInterface> _eventCommandMap;

        public EventHandlers(BankContext bankContext)
        {
            _bankContext = bankContext;
            _eventCommandMap = new Dictionary<Type, ICommandInterface>()
            {
                { typeof(AccountCreatedEvent), new AccountCreateCommand(_bankContext) },
                { typeof(AmountWithdrawnEvent), new AmountWithdrawCommand(_bankContext) },
                { typeof(AmountDepositedEvent), new AmountDepositCommand(_bankContext) },
                { typeof(AccountDeletedEvent), new AccountDeleteCommand(_bankContext) },

            };
        }

        public async Task handleGenericEvent<T>(ResolvedEvent evnt, CancellationToken cancellationToken)
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
