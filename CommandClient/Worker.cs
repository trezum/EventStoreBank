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

        private Task WithdrawAmountAsync()
        {
            throw new NotImplementedException();
        }

        private Task TransferAmountAsync()
        {
            throw new NotImplementedException();
        }

        private Task DepositAmountAsync()
        {
            throw new NotImplementedException();
        }

        private Task DeleteAccountAsync()
        {
            throw new NotImplementedException();
        }

        private async Task CreateAccountAsync()
        {
            Console.WriteLine("Enter owner name:");
            var ownerName = Console.ReadLine();

            _currentAccountId = Guid.NewGuid();

            AccountCreated evt = new AccountCreated()
            {
                AggregateId = _currentAccountId.Value.ToString(),
                OwnerName = ownerName
            };

            await WriteEventAsync(evt);
        }

        private async Task WriteEventAsync(AccountCreated evt)
        {

            // TODO: Optimistic concurrency
            // https://developers.eventstore.com/clients/grpc/appending-events/#handling-concurrency
            var eventData = new EventData(
                Uuid.NewUuid(),
                evt.GetType().Name,
                JsonSerializer.SerializeToUtf8Bytes(evt));

            await _eventStoreClient.AppendToStreamAsync(
            "Account-" + _currentAccountId.Value.ToString(),
            StreamState.Any,
            new[] { eventData },
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