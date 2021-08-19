using Commands;
using Events;
using EventStore.Client;
using Model;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReadModelUpdater
{
    public class EventHandelers
    {
        private readonly BankContext _bankContext;

        public EventHandelers(BankContext bankContext)
        {
            _bankContext = bankContext;
        }

        public async Task handleIfAmountWithdrawn(ResolvedEvent evnt)
        {
            if (evnt.Event.EventType == typeof(AmountWithdrawn).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AmountWithdrawn>(evnt.Event.Data.Span);
                Console.WriteLine("Calling AmountWithdrawn command.");
                var cmd = new AmountWithdrawCommand(_bankContext);
                await cmd.ExecuteAsync(eventObject);
            }
        }

        public async Task handleIfAmountDeposited(ResolvedEvent evnt)
        {
            if (evnt.Event.EventType == typeof(AmountDeposited).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AmountDeposited>(evnt.Event.Data.Span);
                Console.WriteLine("Calling AmountDeposited command.");
                var cmd = new AmountDepositCommand(_bankContext);
                await cmd.ExecuteAsync(eventObject);
            }
        }

        public async Task handleIfAccountDeleted(ResolvedEvent evnt)
        {
            if (evnt.Event.EventType == typeof(AccountDeleted).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AccountDeleted>(evnt.Event.Data.Span);
                Console.WriteLine("Calling AccountDeleted command.");
                var cmd = new AccountDeleteCommand(_bankContext);
                await cmd.ExecuteAsync(eventObject);
            }
        }

        public async Task handleIfAccountCreated(ResolvedEvent evnt)
        {
            if (evnt.Event.EventType == typeof(AccountCreated).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AccountCreated>(evnt.Event.Data.Span);
                Console.WriteLine("Calling AccountCreated command.");
                var cmd = new AccountCreateCommand(_bankContext);
                await cmd.ExecuteAsync(eventObject);
            }
        }
    }
}
