using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Events;
using EventStore.Client;
using Microsoft.Extensions.Hosting;

namespace CommandClient
{
    public class CommandClientWorker : BackgroundService
    {
        // TODO: Optimistic concurrency
        // https://developers.eventstore.com/clients/grpc/appending-events/#handling-concurrency
        // If an event has been sent it is the truth, so it should be denied here.

        private readonly EventStoreClient _eventStoreClient;

        // If null no account is "selected in the client cli"
        private Guid? _currentAccountId;
        private CancellationToken _stoppingToken;

        public CommandClientWorker(EventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
            _currentAccountId = null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            while (!_stoppingToken.IsCancellationRequested)
            {
                PrintMenu();
                await MenuSelectionAsync();
            }
        }

        private void PrintMenu()
        {
            Console.WriteLine("Select a function by pressing the corresponding number: ");
            Console.WriteLine("(0) Create Account");

            if (_currentAccountId.HasValue)
            {
                Console.WriteLine("(1) Delete Account");
                Console.WriteLine("(2) Deposit Amount");
                Console.WriteLine("(3) Transfer Amount");
                Console.WriteLine("(4) Withdraw Account");
            }
        }

        private async Task MenuSelectionAsync()
        {
            var selection = Console.ReadKey();
            Console.Clear();

            if (selection.KeyChar == '0')
            {
                await CreateAccountAsync();
            }

            else if (_currentAccountId.HasValue)
            {
                if (selection.KeyChar == '1')
                {
                    await DeleteAccountAsync();
                }
                else if (selection.KeyChar == '2')
                {
                    await DepositAmountAsync();
                }
                else if (selection.KeyChar == '3')
                {
                    await TransferAmountAsync();
                }
                else if (selection.KeyChar == '4')
                {
                    await WithdrawAmountAsync();
                }
            }
        }

        private async Task WithdrawAmountAsync()
        {
            var amount = Console.ReadLine();

            decimal decimalAmount;

            if (!decimal.TryParse(amount, out decimalAmount))
            {
                return;
            }

            var amountWithdrawn = new AmountWithdrawn()
            {
                AggregateId = _currentAccountId.Value,
                Amount = decimalAmount,
            };

            await _eventStoreClient.AppendToStreamAsync(
                "Account-" + _currentAccountId.Value.ToString(),
                StreamState.StreamExists,
                new[] { new EventData(
                    Uuid.NewUuid(),
                    amountWithdrawn.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(amountWithdrawn))
                },
                cancellationToken: _stoppingToken);
        }

        private async Task TransferAmountAsync()
        {
            // This should be done with two events using optimistic concurrency for both, a withdrawal from one account and a deposit to another.
            // if done in one event it will only show up in one of the account streams, 
            // maybe a deposit and a withdaw should have a source or destionation added

            throw new NotImplementedException();
        }

        private async Task DepositAmountAsync()
        {
            var amount = Console.ReadLine();

            decimal decimalAmount;

            if (!decimal.TryParse(amount, out decimalAmount))
            {
                return;
            }

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
                },
                cancellationToken: _stoppingToken);
        }

        private async Task DeleteAccountAsync()
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
                },
                cancellationToken: _stoppingToken);
            _currentAccountId = null;
        }

        private async Task CreateAccountAsync()
        {
            Console.WriteLine("Enter owner name:");
            var ownerName = Console.ReadLine();

            _currentAccountId = Guid.NewGuid();

            var accountCreated = new AccountCreated()
            {
                AggregateId = _currentAccountId.Value,
                OwnerName = ownerName
            };

            await _eventStoreClient.AppendToStreamAsync(
                "Account-" + _currentAccountId.Value.ToString(),
                StreamState.NoStream,
                new[] { new EventData(
                    Uuid.NewUuid(),
                    accountCreated.GetType().Name,
                    JsonSerializer.SerializeToUtf8Bytes(accountCreated))
                },
                cancellationToken: _stoppingToken);
        }


    }
}