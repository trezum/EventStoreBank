using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Model;
using System.Reflection;
using Commands;
using Events;

namespace ReadModelUpdater
{
    public static class Program
    {
        private static Task Main(string[] args) =>
            CreateHostBuilder(args).Build().RunAsync();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddHostedService<ReadModelUpdateWorker>()
                            .AddDbContext<BankContext>()
                            .AddEventStoreClient("esdb://localhost:2113?tls=false")
                            .AddScoped(typeof(EventHandlers))
                            .RegisterCommands()
                );
        private static IServiceCollection RegisterCommands(this IServiceCollection services)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(AccountCreateCommand));

            if (assembly != null)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName != null && type.FullName.EndsWith("Command"))
                    {
                        services.AddScoped(type);
                    }
                }
            }
            return services;
        }
    }
}
