using Events;
using EventStore.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EvtFacade
{
    public class EventFacade
    {

        private readonly EventStoreClient _eventStoreClient;
        private const string _accountStreamPrefix = "Account-";

        public EventFacade(EventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        private async Task AppendToStream<T>(Guid accountId, long expectedStreamRevision, T evt)
        {
            await _eventStoreClient.AppendToStreamAsync(
                _accountStreamPrefix + accountId.ToString(),
                StreamRevision.FromInt64(expectedStreamRevision),
                new[] { new EventData(
                    Uuid.NewUuid(),
                    evt.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(evt))
                });
        }

        public async Task WithdrawAmountAsync(Guid accountId, long expectedStreamRevision, decimal decimalAmount)
        {
            var amountWithdrawn = new AmountWithdrawnEvent()
            {
                AggregateId = accountId,
                Destination = null,
                Amount = decimalAmount
            };

            await AppendToStream(accountId, expectedStreamRevision, amountWithdrawn);
        }


        // Transactions across multiple streams are not supported.
        // https://developers.eventstore.com/clients/dotnet/20.10/appending/#transactions
        // This thread has some surggestions:
        // https://discuss.eventstore.com/t/cross-aggregate-transactions-in-event-store/2357
        public async Task TransferAmountAsync(Guid sourceAccountId, Guid destinationAccount, decimal amount)
        {
            // implement using process manager?
            // This should be done with two events using optimistic concurrency for both, a withdrawal from one account and a deposit to another.
            // if done in one event it will only show up in one of the account streams, 
            // maybe a deposit and a withdaw should have a source or destionation added
            // --->maybe one event is fine, the state could be updated for two accounts.

            //When sending the two events it should be done as an transaction, maybe opposite event should be created for rollback if two cant be transacted.
            await Task.CompletedTask;
            throw new NotImplementedException();
        }



        public async Task DepositAmountAsync(Guid accountId, long expectedStreamRevision, decimal decimalAmount)
        {
            var amountDeposited = new AmountDepositedEvent()
            {
                AggregateId = accountId,
                Amount = decimalAmount,
            };

            await AppendToStream(accountId, expectedStreamRevision, amountDeposited);
        }



        public async Task<long> GetLastVersionForAccount(Guid accountId)
        {
            var events = _eventStoreClient.ReadStreamAsync(
                Direction.Backwards,
                _accountStreamPrefix + accountId.ToString(),
                StreamPosition.End,
                1);

            await foreach (var e in events)
            {
                return e.OriginalEventNumber.ToInt64();
            }
            throw new Exception("Event Version not found!");
        }

        public async Task DeleteAccountAsync(Guid accountId, long expectedStreamRevision)
        {
            var accountDeleted = new AccountDeletedEvent()
            {
                AggregateId = accountId,
            };

            await AppendToStream(accountId, expectedStreamRevision, accountDeleted);
        }

        public async IAsyncEnumerable<string> GetEventJsonForAccount(Guid accountId)
        {
            var events = _eventStoreClient.ReadStreamAsync(
                Direction.Forwards, _accountStreamPrefix + accountId.ToString(),
                StreamPosition.Start);

            // TOOD: add cancelation tokens here:
            // await foreach (var e in events.WithCancellation(token))
            // https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8
            await foreach (var e in events)
            {
                // Minor hack to add type property, should be in a dto if adding a web api
                // Should use one json converter at that time instead of mixing newtonsoft atm it is the easiest way to work with dynamic type
                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(Encoding.UTF8.GetString(e.Event.Data.ToArray()));
                obj.EventType = e.Event.EventType;
                yield return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
        }

        public async Task CreateAccountAsync(Guid accountId, string ownerName)
        {
            var accountCreated = new AccountCreatedEvent()
            {
                AggregateId = accountId,
                OwnerName = ownerName,
            };

            await AppendToStream(accountId, -1, accountCreated);
        }
    }
}
