using Commands;
using Queries;
using System.Reflection;

namespace Api
{
    public static class CommandQueryExtensions
    {
        public static IServiceCollection RegisterQueries(this IServiceCollection services)
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

        public static IServiceCollection RegisterCommands(this IServiceCollection services)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(AccountCreateCommand));

            if (assembly != null)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName != null && type.FullName.EndsWith("Command"))
                    {
                        services.AddTransient(type);
                    }
                }
            }
            return services;
        }
    }
}
