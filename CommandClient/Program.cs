using EvtFacade;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Model;
using Queries;
using System.Reflection;
using System.Threading.Tasks;

namespace Client
{
    public static class Program
    {
        public static Task Main(string[] args) =>
            CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddHostedService<ClientWorker>()
                            .AddDbContext<BankContext>()
                            .AddEventStoreClient("esdb://localhost:2113?tls=false")
                            .AddSingleton(typeof(EventFacade))
                            .RegisterQueries()
                );

        private static IServiceCollection RegisterQueries(this IServiceCollection services)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(FirstTenAccountsQuery));

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
