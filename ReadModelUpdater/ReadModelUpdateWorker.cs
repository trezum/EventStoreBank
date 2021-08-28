using Commands;
using EventStore.Client;
using Microsoft.Extensions.Hosting;
using Model;
using Queries;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReadModelUpdater
{
    internal class ReadModelUpdateWorker : BackgroundService
    {
        private readonly EventStoreClient _eventStoreClient;
        private CancellationToken _cancellationToken;
        private readonly EventHandlers _eventHandelers;
        private readonly BankContext _context;

        public ReadModelUpdateWorker(EventStoreClient eventStoreClient, EventHandlers eventHandelers, BankContext bankContext)
        {
            _eventStoreClient = eventStoreClient;
            _eventHandelers = eventHandelers;
            _context = bankContext;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Info on cancellationtokens
            // https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads
            // TODO: For web api: https://stackoverflow.com/questions/64205147/addscoped-dependency-with-a-cancellationtoke
            _cancellationToken = cancellationToken;

            // Creating or loading checkpoing as needed.
            Position checkpoint;
            var dbCheckpoint = await new GetCheckpointQuery(_context).ExecuteAsync(_cancellationToken);
            if (dbCheckpoint == null)
            {
                checkpoint = Position.Start;
                await new CreateCheckpointCommand(_context).ExecuteAsync(checkpoint.CommitPosition, checkpoint.PreparePosition, _cancellationToken);
            }
            else
            {
                checkpoint = new Position(dbCheckpoint.CommitPosition, dbCheckpoint.PreparePosition);
            }

            await _eventStoreClient.SubscribeToAllAsync(
                checkpoint,
                subscriptionDropped: async (subscription, reason, exception) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("-----------------------------------------------------------------------------------------");
                    Console.WriteLine($"Subscription was dropped due to {reason}. {exception}");
                    if (reason != SubscriptionDroppedReason.Disposed)
                    {
                        // Resubscribe if the client didn't stop the subscription
                        // Should load a new checkpoint from the database
                        //Resubscribe(checkpoint); 
                    }
                    Console.ForegroundColor = ConsoleColor.Gray;
                },
                eventAppeared: async (subscription, evnt, cancellationToken) =>
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("-----------------------------------------------------------------------------------------");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"Received event {evnt.OriginalEventNumber}@{evnt.OriginalStreamId}@{evnt.Event.EventType}");
                    Console.WriteLine(Encoding.UTF8.GetString(evnt.Event.Data.ToArray()));

                    // If an event has been sent it is the truth, so it should be denied here.
                    // Could not have a microservice for each because the events should be processed in the correct order.
                    // TODO: Make a generic method for this.
                    await _eventHandelers.handleIfAccountCreated(evnt, cancellationToken);
                    await _eventHandelers.handleIfAccountDeleted(evnt, cancellationToken);
                    await _eventHandelers.handleIfAmountDeposited(evnt, cancellationToken);
                    await _eventHandelers.handleIfAmountWithdrawn(evnt, cancellationToken);
                }, filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents()), cancellationToken: _cancellationToken
                );

            Console.WriteLine("Waiting for any key:");
            Console.ReadKey();
        }
    }
}
