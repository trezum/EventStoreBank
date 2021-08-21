using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CommandClient
{
    public class CommandClientWorker : BackgroundService
    {
        // TODO: Optimistic concurrency
        // https://developers.eventstore.com/clients/grpc/appending-events/#handling-concurrency
        // If an event has been sent it is the truth, so it should be denied here.

        private CancellationToken _stoppingToken;
        private readonly EventSender _eventSender;

        public CommandClientWorker(EventSender eventSender)
        {
            _eventSender = eventSender;
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

            if (_eventSender.HasAccountSelected())
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
                Console.WriteLine("Enter owner name:");
                var ownerName = Console.ReadLine();

                await _eventSender.CreateAccountAsync(ownerName);

                Console.WriteLine("Account created.");
            }

            else if (_eventSender.HasAccountSelected())
            {
                if (selection.KeyChar == '1')
                {
                    await _eventSender.DeleteAccountAsync();
                    Console.WriteLine("Account deleted.");
                }
                else if (selection.KeyChar == '2')
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
            }
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
        private static int GetPositiveIntFromUser()
        {
            while (true)
            {
                int input;
                if (int.TryParse(Console.ReadLine(), out input) && input > 0)
                {
                    return input;
                }
                Console.WriteLine("Indtast venligst et positivt heltal mindre end: " + int.MaxValue);
            }
        }

    }
}