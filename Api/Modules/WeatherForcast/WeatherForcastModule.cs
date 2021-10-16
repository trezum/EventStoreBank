namespace Api.Modules.WeatherForcast
{
    public class WeatherForcastModule : IModule
    {
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("WeatherForecast", () => {
                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
                }).ToArray();
            });
            return endpoints;
        }

        public IServiceCollection RegisterModule(IServiceCollection services)
        {
            return services;            
        }
    }
}
