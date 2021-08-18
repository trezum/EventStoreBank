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
                    // Handle using delegates?
                    // Could have a microservice for each if needed
                    // Use EventStoreDB checkpoint to know where to read from in the all-stream
                    Console.WriteLine($"Received event {evnt.OriginalEventNumber}@{evnt.OriginalStreamId}@{evnt.Event.EventType}");
                    Console.WriteLine(Encoding.UTF8.GetString(evnt.Event.Data.ToArray()));

                    await handleIfAccountCreated(evnt);
                    await handleIfAccountDeleted(evnt);
                    await handleIfAmountDeposited(evnt);
                    await handleIfAmountWithdrawn(evnt);

                }, filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents())
                );

            Console.WriteLine("Waiting for any key:");
            Console.ReadKey();
        }

        private async Task handleIfAmountWithdrawn(ResolvedEvent evnt)
        {
            if (evnt.Event.EventType == typeof(AmountWithdrawn).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AmountWithdrawn>(evnt.Event.Data.Span);
                Console.WriteLine("Calling AmountWithdrawn command.");
                await Task.Yield();
            }
        }

        private async Task handleIfAmountDeposited(ResolvedEvent evnt)
        {
            if (evnt.Event.EventType == typeof(AmountDeposited).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AmountDeposited>(evnt.Event.Data.Span);
                Console.WriteLine("Calling AmountDeposited command.");
                await Task.Yield();
            }
        }

        private async Task handleIfAccountDeleted(ResolvedEvent evnt)
        {
            if (evnt.Event.EventType == typeof(AccountDeleted).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AccountDeleted>(evnt.Event.Data.Span);
                Console.WriteLine("Calling AccountDeleted command.");
                await Task.Yield();
            }
        }

        private async Task handleIfAccountCreated(ResolvedEvent evnt)
        {
            if (evnt.Event.EventType == typeof(AccountCreated).Name)
            {
                var eventObject = JsonSerializer.Deserialize<AccountCreated>(evnt.Event.Data.Span);
                Console.WriteLine("Calling AccountCreated command.");
                await Task.Yield();
            }
        }
    }
}
