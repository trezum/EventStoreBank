using Events;
using EventStore.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommandClient
{
    public class EventFacade
    {
        // If null no account is selected.
        // TODO: Should be moved to the frontend if an api is introduced.
        private Guid? _currentAccountId;
        private long _eventVersion;
        private readonly EventStoreClient _eventStoreClient;
        private const string _accountStreamPrefix = "Account-";

        public EventFacade(EventStoreClient eventStoreClient)
        {
            _currentAccountId = null;
            _eventStoreClient = eventStoreClient;
        }


        internal async Task AppendToStream<T>(T evt)
        {
            await _eventStoreClient.AppendToStreamAsync(
                _accountStreamPrefix + _currentAccountId.Value.ToString(),
                StreamRevision.FromInt64(_eventVersion - 1),
                new[] { new EventData(
                    Uuid.NewUuid(),
                    evt.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(evt))
                });
        }

        internal async Task WithdrawAmountAsync(decimal decimalAmount)
        {
            _eventVersion++;
            var amountWithdrawn = new AmountWithdrawnEvent()
            {
                AggregateId = _currentAccountId.Value,
                Destination = null,
                Amount = decimalAmount,
                EventVersion = _eventVersion,
            };

            await AppendToStream(amountWithdrawn);
        }

        internal void DeselectAccount()
        {
            _currentAccountId = null;
        }

        // Transactions across multiple streams are not supported.
        // https://developers.eventstore.com/clients/dotnet/20.10/appending/#transactions
        // This thread has some surggestions:
        // https://discuss.eventstore.com/t/cross-aggregate-transactions-in-event-store/2357
        internal async Task TransferAmountAsync(Guid destinationAccount)
        {
            _eventVersion++;
            // implement using process manager?
            // This should be done with two events using optimistic concurrency for both, a withdrawal from one account and a deposit to another.
            // if done in one event it will only show up in one of the account streams, 
            // maybe a deposit and a withdaw should have a source or destionation added

            //When sending the two events it should be done as an transaction, maybe opposite event should be created for rollback if two cant be transacted.

            throw new NotImplementedException();
        }

        internal bool HasAccountSelected()
        {
            return _currentAccountId != null;
        }

        internal async Task DepositAmountAsync(decimal decimalAmount)
        {
            _eventVersion++;
            var amountDeposited = new AmountDepositedEvent()
            {
                AggregateId = _currentAccountId.Value,
                Amount = decimalAmount,
                EventVersion = _eventVersion,
            };

            await AppendToStream(amountDeposited);
        }

        internal async Task SelectAccountAsync(Guid newSelection)
        {
            _currentAccountId = newSelection;
            _eventVersion = await GetLastVersionForCurrentAccount();
        }

        internal async Task<long> GetLastVersionForCurrentAccount()
        {
            var events = _eventStoreClient.ReadStreamAsync(
                Direction.Backwards,
                _accountStreamPrefix + _currentAccountId.Value.ToString(),
                StreamPosition.End,
                1);

            await foreach (var e in events)
            {
                return e.OriginalEventNumber.ToInt64();
            }
            throw new Exception("Event Version not found!");
        }

        internal async Task DeleteAccountAsync()
        {
            _eventVersion++;
            var accountDeleted = new AccountDeletedEvent()
            {
                AggregateId = _currentAccountId.Value,
                EventVersion = _eventVersion,
            };

            await AppendToStream(accountDeleted);
            _currentAccountId = null;
        }

        internal async IAsyncEnumerable<string> GetEventJsonFor()
        {
            var events = _eventStoreClient.ReadStreamAsync(
                Direction.Forwards,
                _accountStreamPrefix + _currentAccountId.Value.ToString(),
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

        internal async Task CreateAccountAsync(string ownerName)
        {
            _currentAccountId = Guid.NewGuid();
            _eventVersion = 0;
            var accountCreated = new AccountCreatedEvent()
            {
                AggregateId = _currentAccountId.Value,
                OwnerName = ownerName,
                EventVersion = _eventVersion,
            };

            await AppendToStream(accountCreated);
        }
    }
}
