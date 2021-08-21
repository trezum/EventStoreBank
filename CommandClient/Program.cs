using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Model;
using Queries;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandClient
{
    static class Program
    {
        static Task Main(string[] args) =>
            CreateHostBuilder(args).Build().RunAsync();

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddHostedService<ClientWorker>()
                            .AddDbContext<BankContext>()
                            .AddEventStoreClient("esdb://localhost:2113?tls=false")
                            .AddSingleton(typeof(EventSender))
                            .RegisterQueries()
                );

        private static IServiceCollection RegisterQueries(this IServiceCollection services)
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(TopTenAccountsQuery));

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
