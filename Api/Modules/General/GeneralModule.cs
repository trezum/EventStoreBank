using EventFacade;
using Model;
namespace Api.Modules.General
{
    public class GeneralModule : IModule
    {
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            //This module should not have any endpoints, it is used to register often used services.
            return endpoints;
        }

        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            services.AddDbContext<BankContext>();
            services.AddEventStoreClient("esdb://localhost:2113?tls=false");
            services.AddSingleton(typeof(AccountEventFacade));
            services.RegisterQueries();
            services.RegisterCommands();
            return services;
        }
    }
}
