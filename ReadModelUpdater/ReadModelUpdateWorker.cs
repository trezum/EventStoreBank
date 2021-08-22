using EventStore.Client;
using Microsoft.Extensions.Hosting;
using System;
using System.Text;
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
        private readonly EventHandelers _eventHandelers;

        public ReadModelUpdateWorker(EventStoreClient eventStoreClient, EventHandelers eventHandelers)
        {
            _eventStoreClient = eventStoreClient;
            _eventHandelers = eventHandelers;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            //TODO: Use EventStoreDB checkpoint to know where to read from in the all-stream
            await _eventStoreClient.SubscribeToAllAsync(
                async (subscription, evnt, cancellationToken) =>
                {
                    // Handle using delegates?
                    // Could have a microservice for each if needed
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("-----------------------------------------------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"Received event {evnt.OriginalEventNumber}@{evnt.OriginalStreamId}@{evnt.Event.EventType}");
                    Console.WriteLine(Encoding.UTF8.GetString(evnt.Event.Data.ToArray()));

                    await _eventHandelers.handleIfAccountCreated(evnt);
                    await _eventHandelers.handleIfAccountDeleted(evnt);
                    await _eventHandelers.handleIfAmountDeposited(evnt);
                    await _eventHandelers.handleIfAmountWithdrawn(evnt);

                }, filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents())
                );

            Console.WriteLine("Waiting for any key:");
            Console.ReadKey();
        }
    }
}
