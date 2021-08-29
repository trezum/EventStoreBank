using Commands;
using EventStore.Client;
using Microsoft.Extensions.Hosting;
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
        private readonly CreateCheckpointCommand _createCheckpointCommand;
        private readonly GetCheckpointQuery _getCheckpointQuery;

        public ReadModelUpdateWorker(
            EventStoreClient eventStoreClient,
            EventHandlers eventHandelers,
            CreateCheckpointCommand createCheckpointCommand,
            GetCheckpointQuery getCheckpointQuery)

        {
            _eventStoreClient = eventStoreClient;
            _eventHandelers = eventHandelers;
            _createCheckpointCommand = createCheckpointCommand;
            _getCheckpointQuery = getCheckpointQuery;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Info on cancellationtokens
            // https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads
            // TODO: For web api: https://stackoverflow.com/questions/64205147/addscoped-dependency-with-a-cancellationtoke
            _cancellationToken = cancellationToken;

            // Creating or loading checkpoing as needed.
            Position checkpoint;
            var dbCheckpoint = await _getCheckpointQuery.ExecuteAsync(_cancellationToken);
            if (dbCheckpoint == null)
            {
                checkpoint = Position.Start;
                await _createCheckpointCommand.ExecuteAsync(checkpoint.CommitPosition, checkpoint.PreparePosition, _cancellationToken);
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

                    await _eventHandelers.handleEvents(evnt, cancellationToken);

                }, filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents()), cancellationToken: _cancellationToken
                );

            Console.WriteLine("Waiting for any key:");
            Console.ReadKey();
        }
    }
}
