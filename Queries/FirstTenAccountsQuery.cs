using Microsoft.EntityFrameworkCore;
using Model;
using System.Linq;
using System.Threading.Tasks;

namespace Queries
{
    public class FirstTenAccountsQuery
    {
        private readonly BankContext _context;

        public FirstTenAccountsQuery(BankContext context)
        {
            _context = context;
        }

        public async Task<Account[]> Execute()
        {
            return await _context.Accounts.Take(10).ToArrayAsync();
        }
    }
}
