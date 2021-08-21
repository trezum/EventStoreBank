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
        private long _eventVersion;
        private readonly EventStoreClient _eventStoreClient;
        private const string _accountStreamPrefix = "Account-";

        public EventSender(EventStoreClient eventStoreClient)
        {
            _currentAccountId = null;
            _eventStoreClient = eventStoreClient;
        }

        internal async Task WithdrawAmountAsync(decimal decimalAmount)
        {
            _eventVersion++;
            var amountWithdrawn = new AmountWithdrawn()
            {
                AggregateId = _currentAccountId.Value,
                Destination = null,
                Amount = decimalAmount,
                EventVersion = _eventVersion,
            };

            await _eventStoreClient.AppendToStreamAsync(
                _accountStreamPrefix + _currentAccountId.Value.ToString(),
                StreamRevision.FromInt64(_eventVersion - 1),
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
            _eventVersion++;
            // This should be done with two events using optimistic concurrency for both, a withdrawal from one account and a deposit to another.
            // if done in one event it will only show up in one of the account streams, 
            // maybe a deposit and a withdaw should have a source or destionation added

            //When sending the two events it should be done as an transaction, maybe opposite event should be created for rollback if two cant be transacted.

            // Todo: Include event version in read in order to reduce eventual concistency issues
            // https://youtu.be/FKFu78ZEIi8?t=1771
            // When querying for updated data the desired all eventVersion, from the write, could be sent to/with the query so it can wait for data to be updated.
            throw new NotImplementedException();
        }

        internal bool HasAccountSelected()
        {
            return _currentAccountId != null;
        }

        internal async Task DepositAmountAsync(decimal decimalAmount)
        {
            _eventVersion++;
            var amountDeposited = new AmountDeposited()
            {
                AggregateId = _currentAccountId.Value,
                Amount = decimalAmount,
                EventVersion = _eventVersion,
            };

            await _eventStoreClient.AppendToStreamAsync(
                _accountStreamPrefix + _currentAccountId.Value.ToString(),
                StreamRevision.FromInt64(_eventVersion - 1),
                new[] { new EventData(
                    Uuid.NewUuid(),
                    amountDeposited.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(amountDeposited))
                });
        }

        internal Task SelectAccountAsync(Guid newSelection)
        {
            _currentAccountId = newSelection;

            //TODO: On concurrency error drop selected account and EventVersion.


            //Request event version
            throw new NotImplementedException();
        }

        internal async Task DeleteAccountAsync()
        {
            _eventVersion++;
            var accountDeleted = new AccountDeleted()
            {
                AggregateId = _currentAccountId.Value,
                EventVersion = _eventVersion,
            };

            await _eventStoreClient.AppendToStreamAsync(
                    _accountStreamPrefix + _currentAccountId.Value.ToString(),
                    StreamRevision.FromInt64(_eventVersion - 1),
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
            _eventVersion = 0;
            var accountCreated = new AccountCreated()
            {
                AggregateId = _currentAccountId.Value,
                OwnerName = ownerName,
                EventVersion = _eventVersion,
            };

            await _eventStoreClient.AppendToStreamAsync(
                _accountStreamPrefix + _currentAccountId.Value.ToString(),
                StreamState.NoStream,
                new[] { new EventData(
                    Uuid.NewUuid(),
                    accountCreated.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(accountCreated))
                });
        }
    }
}
