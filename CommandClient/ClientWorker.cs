using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Queries;

namespace CommandClient
{
    public class ClientWorker : BackgroundService
    {
        // TODO: Optimistic concurrency
        // https://developers.eventstore.com/clients/grpc/appending-events/#handling-concurrency
        // If an event has been sent it is the truth, so it should be denied here.

        private CancellationToken _stoppingToken;
        private readonly EventSender _eventSender;
        private readonly TopTenAccountsQuery _topTenAccountsQuery;

        public ClientWorker(EventSender eventSender, TopTenAccountsQuery topTenAccountsQuery)
        {
            _eventSender = eventSender;
            _topTenAccountsQuery = topTenAccountsQuery;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;
            while (!_stoppingToken.IsCancellationRequested)
            {
                Console.Clear();
                PrintMenu();
                await MenuSelectionAsync();
            }
        }

        private void PrintMenu()
        {
            Console.WriteLine("Select a function by pressing the corresponding number: ");
            Console.WriteLine("(0) Create Account");
            Console.WriteLine("(1) Select Account");
            if (_eventSender.HasAccountSelected())
            {

                Console.WriteLine("(2) Deposit Amount");
                Console.WriteLine("(3) Transfer Amount");
                Console.WriteLine("(4) Withdraw Account");
                Console.WriteLine("(5) Delete Account");
                //Console.WriteLine("(6) Account History");
            }
        }

        private async Task MenuSelectionAsync()
        {
            var selection = Console.ReadKey();
            Console.Clear();

            if (selection.KeyChar == '0')
            {
                Console.WriteLine("Enter owner name:");
                var ownerName = Console.ReadLine();

                await _eventSender.CreateAccountAsync(ownerName);

                Console.WriteLine("Account created.");
            }
            else if (selection.KeyChar == '1')
            {
                Guid account = await GetAccountSelectionFromUser();
                await _eventSender.SelectAccountAsync(account);
                Console.WriteLine("Account selected.");
            }
            else if (_eventSender.HasAccountSelected())
            {

                if (selection.KeyChar == '2')
                {
                    Console.WriteLine("Enter amount to deposit:");
                    await _eventSender.DepositAmountAsync(GetDecimalFromUser());
                    Console.WriteLine("Amount deposited.");
                }
                else if (selection.KeyChar == '3')
                {
                    var destinationAccount = Guid.NewGuid();
                    await _eventSender.TransferAmountAsync(destinationAccount);
                }
                else if (selection.KeyChar == '4')
                {
                    Console.WriteLine("Enter amount to withdraw:");
                    await _eventSender.WithdrawAmountAsync(GetDecimalFromUser());
                    Console.WriteLine("Amount withdrawn.");
                }
                else if (selection.KeyChar == '5')
                {
                    await _eventSender.DeleteAccountAsync();
                    Console.WriteLine("Account deleted.");
                }
                else
                {
                    Console.WriteLine("No valid key pressed, try again.");
                    Thread.Sleep(1500);
                }
            }
        }

        private async Task<Guid> GetAccountSelectionFromUser()
        {
            // Query top 10 accounts
            var accounts = await _topTenAccountsQuery.Execute();
            // Display account table
            Console.WriteLine("Select an account by pressing the corrorsponding number.");
            for (int i = 0; i < accounts.Length; i++)
            {
                Console.WriteLine("({0}) - {1} - {2} - {3} - {4}", i, accounts[i].Id.ToString(), accounts[i].OwnerName, accounts[i].Balance, accounts[i].EventVersion);
            }

            // Get selection from user
            return accounts[GetIntFromUser()].Id;
        }

        private static decimal GetDecimalFromUser()
        {
            while (true)
            {
                decimal input;
                if (decimal.TryParse(Console.ReadLine(), out input))
                {
                    return input;
                }
                Console.WriteLine("Enter a decimal number between {0} and {1} ", decimal.MinValue, decimal.MaxValue);
            }
        }
        private static int GetIntFromUser(int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                int input;
                if (int.TryParse(Console.ReadLine(), out input) && input > min && input < max)
                {
                    return input;
                }
                Console.WriteLine("Enter a number between {0} and {1}", min, max);
            }
        }

    }
}