using EventFacade;
using System.Net;

namespace Api.Modules.Account
{
    public class AccountCreateModule : IModule
    {
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("account/{accountId}", async (Guid accountId, string ownerName, HttpContext http, AccountEventFacade facade) =>
            {
                if (!http.Request.RouteValues.TryGetValue("accountId", out var id))
                {
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                if (String.IsNullOrWhiteSpace(ownerName))
                {
                    await http.Response.WriteAsync("EmptyName");
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                try
                {
                    await facade.CreateAccountAsync(accountId, ownerName);
                    http.Response.StatusCode = (int)HttpStatusCode.OK;
                    return;
                }
                catch (Exception)
                {
                    await http.Response.WriteAsync("Exception");
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
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
