using EventFacade;
using System.Net;

namespace Api.Modules.Account
{
    public class AccountRevisionModule : IModule
    {
        //TODO: Make this retun proper JSON
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("account/revision/{accountId}", async (Guid accountId, HttpContext http, AccountEventFacade facade) =>
            {
                if (!http.Request.RouteValues.TryGetValue("accountId", out var id))
                {
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return -1;
                }
                try
                {
                    return await facade.GetLastVersionForAccount(accountId);
                }
                catch (Exception)
                {
                    http.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await http.Response.WriteAsync("AccountNotFound");
                    return -1;
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
