using EventStore.Client;
using EventFacade;
using System.Net;

namespace Api.Modules.Account
{
    public class AccountDepositAmountModule : IModule
    {
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("account/deposit/", async (Guid accountId, long expectedVersion, decimal amount, HttpContext http, AccountEventFacade facade) =>
            {
                try
                {
                    await facade.DepositAmountAsync(accountId, expectedVersion, amount);
                }
                catch (WrongExpectedVersionException)
                {
                    await http.Response.WriteAsync("WrongExpectedVersion");
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                http.Response.StatusCode = (int)HttpStatusCode.OK;

            });
            return endpoints;
        }

        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            return services;
        }
    }
}
