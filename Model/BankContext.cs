using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Model
{
    public class BankContext : DbContext
    {
        public static readonly ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss ";
                    }));
        public DbSet<Account> Accounts { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=localhost,1433;User ID=sa;Password=yourStrong(!)Password;Database=EventStoreBankState;MultipleActiveResultSets=true");
            options.EnableSensitiveDataLogging();
            options.UseLoggerFactory(loggerFactory);
        }

    }
}
