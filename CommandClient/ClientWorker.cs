using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Queries;

namespace CommandClient
{
    public class ClientWorker : BackgroundService
    {
        private CancellationToken _stoppingToken;
        private readonly EventFacade _eventSender;
        private readonly TopTenAccountsQuery _topTenAccountsQuery;

        // TODO: Figure out if a refactor is needed so the client works with commands and validators instead of events.
        public ClientWorker(EventFacade eventSender, TopTenAccountsQuery topTenAccountsQuery)
        {
            _eventSender = eventSender;
            _topTenAccountsQuery = topTenAccountsQuery;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;
            while (!_stoppingToken.IsCancellationRequested)
            {
                try
                {
                    PrintMenu();
                    await MenuSelectionAsync();
                    Console.Clear();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.GetType());
                    Console.WriteLine(e.Message);
                    Console.WriteLine("An error occured, someone else probably edited the same account, try again.");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    _eventSender.DeselectAccount();
                }
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
                //Console.WriteLine("(6) Account History"); // EventDTO? Maybe just the JSON?
                //Console.WriteLine("(7) Enable/Disable EF Core Logging");
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
                Console.WriteLine("({0}) - {1} - {2} - {3} - {4}", i, accounts[i].Id.ToString(), accounts[i].OwnerName, accounts[i].EventVersion, accounts[i].Balance);
            }

            // Get selection from user
            return accounts[GetDigitFromUser(0, accounts.Length - 1)].Id;
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

        private static int GetDigitFromUser(int min = 0, int max = 9)
        {
            while (true)
            {
                var key = Console.ReadKey();
                var charr = char.GetNumericValue(key.KeyChar).ToString();

                int input;
                if (int.TryParse(charr, out input) && input >= min && input <= max)
                {
                    return input;
                }
                Console.WriteLine("Enter a number between {0} and {1}", min, max);
            }
        }
    }
}