using Events;
using EventStore.Client;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommandClient
{
    public class EventSender
    {
        // If null no account is selected.
        private Guid? _currentAccountId;
        private readonly EventStoreClient _eventStoreClient;

        public EventSender(EventStoreClient eventStoreClient)
        {
            _currentAccountId = null;
            _eventStoreClient = eventStoreClient;
        }

        internal async Task WithdrawAmountAsync(decimal decimalAmount)
        {
            var amountWithdrawn = new AmountWithdrawn()
            {
                AggregateId = _currentAccountId.Value,
                Destination = null,
                Amount = decimalAmount,
            };

            await _eventStoreClient.AppendToStreamAsync(
                "Account-" + _currentAccountId.Value.ToString(),
                StreamState.StreamExists,
                new[] { new EventData(
                    Uuid.NewUuid(),
                    amountWithdrawn.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(amountWithdrawn))
                });

        }

        // Transactions across multiple streams are not supported.
        // https://developers.eventstore.com/clients/dotnet/20.10/appending/#transactions
        internal async Task TransferAmountAsync(Guid destinationAccount)
        {
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
            var amountDeposited = new AmountDeposited()
            {
                AggregateId = _currentAccountId.Value,
                Amount = decimalAmount,
            };

            await _eventStoreClient.AppendToStreamAsync(
                "Account-" + _currentAccountId.Value.ToString(),
                StreamState.StreamExists,
                new[] { new EventData(
                    Uuid.NewUuid(),
                    amountDeposited.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(amountDeposited))
                });
        }

        internal async Task DeleteAccountAsync()
        {
            var accountDeleted = new AccountDeleted()
            {
                AggregateId = _currentAccountId.Value,
            };

            await _eventStoreClient.AppendToStreamAsync(
                "Account-" + _currentAccountId.Value.ToString(),
                StreamState.StreamExists,
                new[] { new EventData(
                    Uuid.NewUuid(),
                    accountDeleted.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(accountDeleted))
                });
            _currentAccountId = null;
        }

        internal async Task CreateAccountAsync(string ownerName)
        {
            _currentAccountId = Guid.NewGuid();

            var accountCreated = new AccountCreated()
            {
                AggregateId = _currentAccountId.Value,
                OwnerName = ownerName,
                EventVersion = 0,
            };

            await _eventStoreClient.AppendToStreamAsync(
                "Account-" + _currentAccountId.Value.ToString(),
                StreamState.NoStream,
                new[] { new EventData(
                    Uuid.NewUuid(),
                    accountCreated.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(accountCreated))
                });
        }
    }
}
