using EventFacade;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Queries;

namespace Client
{
    public class ClientWorker : BackgroundService
    {
        private CancellationToken _stoppingToken;
        private readonly AccountEventFacade _eventFacade;
        private readonly FirstTenAccountsQuery _topTenAccountsQuery;
        private Guid? _currentAccountId;
        private long _expectedStreamRevision;

        // TODO: Figure out if a refactor is needed so the client works with commands and validators instead of events.
        public ClientWorker(AccountEventFacade eventFacade, FirstTenAccountsQuery topTenAccountsQuery)
        {
            // If null no account is selected.
            _currentAccountId = null;
            _expectedStreamRevision = 0;
            _eventFacade = eventFacade;
            _topTenAccountsQuery = topTenAccountsQuery;
        }

        public void DeselectAccount()
        {
            _currentAccountId = null;
        }
        public bool HasAccountSelected()
        {
            return _currentAccountId != null;
        }

        public async Task SelectAccountAsync(Guid newSelection)
        {
            _currentAccountId = newSelection;
            _expectedStreamRevision = await _eventFacade.GetLastVersionForAccount(_currentAccountId.Value);
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
                    DeselectAccount();
                }
            }
        }
        private void PrintMenu()
        {
            Console.WriteLine("Select a function by pressing the corresponding number: ");
            Console.WriteLine("(0) Create Account");
            Console.WriteLine("(1) Select Account");
            if (HasAccountSelected())
            {
                Console.WriteLine("(2) Deposit Amount");
                Console.WriteLine("(3) Transfer Amount");
                Console.WriteLine("(4) Withdraw Account");
                Console.WriteLine("(5) Delete Account");
                Console.WriteLine("(6) Account History");
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
                _currentAccountId = Guid.NewGuid();
                await _eventFacade.CreateAccountAsync(_currentAccountId.Value, ownerName);
                _expectedStreamRevision = 0;
                Console.WriteLine("Account created.");
            }
            else if (selection.KeyChar == '1')
            {
                Guid account = await GetAccountSelectionFromUser();
                await SelectAccountAsync(account);
                Console.WriteLine("Account selected.");
            }
            else if (HasAccountSelected())
            {
                if (selection.KeyChar == '2')
                {
                    Console.WriteLine("Enter amount to deposit:");
                    await _eventFacade.DepositAmountAsync(_currentAccountId.Value, _expectedStreamRevision, GetDecimalFromUser());
                    _expectedStreamRevision++;
                    Console.WriteLine("Amount deposited.");
                }
                else if (selection.KeyChar == '3')
                {
                    throw new NotImplementedException();

                    //await _eventSender.TransferAmountAsync(_currentAccountId.Value, destinationAccount, );
                    //_expectedStreamRevision++;
                }
                else if (selection.KeyChar == '4')
                {
                    Console.WriteLine("Enter amount to withdraw:");
                    await _eventFacade.WithdrawAmountAsync(_currentAccountId.Value, _expectedStreamRevision, GetDecimalFromUser());
                    _expectedStreamRevision++;
                    Console.WriteLine("Amount withdrawn.");
                }
                else if (selection.KeyChar == '5')
                {
                    await _eventFacade.DeleteAccountAsync(_currentAccountId.Value, _expectedStreamRevision);
                    _expectedStreamRevision++;
                    DeselectAccount();
                    Console.WriteLine("Account deleted.");
                }
                else if (selection.KeyChar == '6')
                {
                    await foreach (var json in _eventFacade.GetEventJsonForAccount(_currentAccountId.Value))
                    {
                        Console.WriteLine(JToken.Parse(json).ToString(Formatting.Indented));
                    }
                    Console.WriteLine("Press any key to continue.");
                    Console.ReadKey();
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
            var accounts = await _topTenAccountsQuery.Execute(_stoppingToken);
            // Display account table
            Console.WriteLine("Select an account by pressing the corrorsponding number.");
            for (int i = 0; i < accounts.Length; i++)
            {
                Console.WriteLine("({0}) - {1} - {2} - {3}", i, accounts[i].Id.ToString(), accounts[i].OwnerName, accounts[i].Balance);
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