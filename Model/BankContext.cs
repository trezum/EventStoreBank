using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Model
{
    public class BankContext : DbContext
    {

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Checkpoint> Checkpoints { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=localhost,1433;User ID=sa;Password=yourStrong(!)Password;Database=EventStoreBankState;MultipleActiveResultSets=true");
            options.EnableSensitiveDataLogging();
            options.UseLoggerFactory(loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Checkpoint>().HasData(new Checkpoint()
            {
                Id = 1,
                CommitPosition = 0,
                PreparePosition = 0
            });
        }

        public static readonly ILoggerFactory loggerFactory =
        LoggerFactory.Create(builder =>
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "hh:mm:ss ";
            }));
    }
}
