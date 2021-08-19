using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Model;
using System.Reflection;
using Queries;
using Commands;

namespace ReadModelUpdater
{
    static class Program
    {
        private static Task Main(string[] args) =>
            CreateHostBuilder(args).Build().RunAsync();

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddHostedService<ReadModelUpdateWorker>()
                            .AddDbContext<BankContext>()
                            .AddEventStoreClient("esdb://localhost:2113?tls=false")
                            .AddScoped(typeof(EventHandelers))
                            //.RegisterQueries()
                            .RegisterCommands());


        private static IServiceCollection RegisterCommands(this IServiceCollection services)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(AccountCreateCommand));

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

        private static IServiceCollection RegisterQueries(this IServiceCollection services)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(IQuery));

            if (assembly != null)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName != null && type.FullName.EndsWith("Query"))
                    {
                        services.AddScoped(type);
                    }
                }
            }
            return services;
        }

    }

}
