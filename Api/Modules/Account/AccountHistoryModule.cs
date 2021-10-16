using EventStore.Client;
using EventFacade;
using System.Net;

namespace Api.Modules.Account
{
    public class AccountHistoryModule : IModule
    {
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("account/history/{accountId}", async (Guid accountId, HttpContext http, AccountEventFacade facade) =>
            {
                if (!http.Request.RouteValues.TryGetValue("accountId", out var id))
                {
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return new string[0];
                }
                try
                {
                    return await facade.GetEventJsonForAccount(accountId).ToArrayAsync();
                }
                catch (StreamNotFoundException)
                {
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await http.Response.WriteAsync("AccountNotFound");
                    return new string[0];
                }
            });
            return endpoints;
        }

        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            return services;
        }
    }
}
