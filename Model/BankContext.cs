using Microsoft.EntityFrameworkCore;

namespace Model
{
    public class BankContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }// = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer("Server=localhost,1433;User ID=sa;Password=yourStrong(!)Password;Database=EventStoreBankState;MultipleActiveResultSets=true");
        }

    }
}
