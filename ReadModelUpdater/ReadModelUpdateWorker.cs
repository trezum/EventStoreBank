using Events;
using EventStore.Client;
using Microsoft.Extensions.Hosting;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ReadModelUpdater
{
    class ReadModelUpdateWorker : BackgroundService
    {
        // TODO: Optimistic concurrency
        // https://developers.eventstore.com/clients/grpc/appending-events/#handling-concurrency
        // If an event has been sent it is the truth, so it should be denied here.

        private readonly EventStoreClient _eventStoreClient;
        private CancellationToken _stoppingToken;

        public ReadModelUpdateWorker(EventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            await _eventStoreClient.SubscribeToAllAsync(
                async (subscription, evnt, cancellationToken) =>
                {
                    // Move handeling cast to the individual methods
                    // Or add a generic method!!!!

                    // handle using delegates?
                    Console.WriteLine($"Received event {evnt.OriginalEventNumber}@{evnt.OriginalStreamId}@{evnt.Event.EventType}");
                    Console.WriteLine(Encoding.UTF8.GetString(evnt.Event.Data.ToArray()));

                    if (evnt.Event.EventType == typeof(AccountCreated).Name)
                    {
                        var eventObject = JsonSerializer.Deserialize<AccountCreated>(evnt.Event.Data.Span);
                        await handleAccountCreated(eventObject);
                    }
                    else if (evnt.Event.EventType == typeof(AccountDeleted).Name)
                    {
                        var eventObject = JsonSerializer.Deserialize<AccountDeleted>(evnt.Event.Data.Span);
                        await handleAccountDeleted(eventObject);
                    }
                    else if (evnt.Event.EventType == typeof(AmountDeposited).Name)
                    {
                        var eventObject = JsonSerializer.Deserialize<AmountDeposited>(evnt.Event.Data.Span);
                        await handleAmountDeposited(eventObject);
                    }
                    else if (evnt.Event.EventType == typeof(AmountTransferred).Name)
                    {
                        var eventObject = JsonSerializer.Deserialize<AmountTransferred>(evnt.Event.Data.Span);
                        await handleAmountTransfered(eventObject);
                    }
                    else if (evnt.Event.EventType == typeof(AmountWithdrawn).Name)
                    {
                        var eventObject = JsonSerializer.Deserialize<AmountWithdrawn>(evnt.Event.Data.Span);
                        await handleAmountWithdrawn(eventObject);
                    }

                }, filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents())
                );

            Console.WriteLine("Waiting for any key:");
            Console.ReadKey();
        }

        private T checkType<T>(EventRecord evnt) where T : EventBase
        {
            if (evnt.EventType == typeof(T).Name)
            {
                return JsonSerializer.Deserialize<T>(evnt.Data.Span);
            }

            return default(T);
        }

        private async Task handleAmountWithdrawn(AmountWithdrawn eventObject)
        {
            Console.WriteLine("Calling AmountWithdrawn command.");
            await Task.Yield();
        }

        private async Task handleAmountTransfered(AmountTransferred eventObject)
        {
            Console.WriteLine("Calling AmountTransferred command.");
            await Task.Yield();
        }

        private async Task handleAmountDeposited(AmountDeposited eventObject)
        {
            Console.WriteLine("Calling AmountDeposited command.");
            await Task.Yield();
        }

        private async Task handleAccountDeleted(AccountDeleted eventObject)
        {
            Console.WriteLine("Calling AccountDeleted command.");
            await Task.Yield();
        }

        private async Task handleAccountCreated(AccountCreated eventObject)
        {
            Console.WriteLine("Calling AccountCreated command.");
            await Task.Yield();
        }
    }
}
