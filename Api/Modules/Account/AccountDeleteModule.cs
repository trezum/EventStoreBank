using EventStore.Client;
using EventFacade;
using System.Net;

namespace Api.Modules.Account
{
    public class AccountDeleteModule : IModule
    {
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("account/{accountId}", async (Guid accountId, long expectedVersion, HttpContext http, AccountEventFacade facade) =>
            {
                if (!http.Request.RouteValues.TryGetValue("accountId", out var id))
                {
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                try
                {
                    await facade.DeleteAccountAsync(accountId, expectedVersion);
                    http.Response.StatusCode = (int)HttpStatusCode.OK;
                    return;
                }
                catch (WrongExpectedVersionException)
                {
                    await http.Response.WriteAsync("WrongExpectedVersion");
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                catch (StreamNotFoundException)
                {
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await http.Response.WriteAsync("AccountNotFound");
                    return;
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
