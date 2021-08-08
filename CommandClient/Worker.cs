using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Events;
using EventStore.Client;
using Microsoft.Extensions.Hosting;

namespace CommandClient
{
    public class Worker : BackgroundService
    {
        // TODO: Optimistic concurrency
        // https://developers.eventstore.com/clients/grpc/appending-events/#handling-concurrency

        private readonly EventStoreClient _eventStoreClient;
        private Guid? _currentAccountId;
        private CancellationToken _stoppingToken;

        public Worker(EventStoreClient eventStoreClient)
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
                AggregateId = _currentAccountId.Value.ToString(),
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
            _currentAccountId = null;
        }

        private async Task TransferAmountAsync()
        {
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
                AggregateId = _currentAccountId.Value.ToString(),
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
            _currentAccountId = null;
        }

        private async Task DeleteAccountAsync()
        {
            var accountDeleted = new AccountDeleted()
            {
                AggregateId = _currentAccountId.Value.ToString(),

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
                AggregateId = _currentAccountId.Value.ToString(),
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

    }
}