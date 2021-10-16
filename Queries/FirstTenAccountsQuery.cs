using Microsoft.EntityFrameworkCore;
using Model;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Queries
{
    // This Query is used by the console client because it can only show ten Accounts atm.
    public class FirstTenAccountsQuery
    {
        private readonly BankContext _context;

        public FirstTenAccountsQuery(BankContext context)
        {
            _context = context;
        }

        public async Task<Account[]> Execute(CancellationToken cancellationToken)
        {
            return await _context.Accounts.Take(10).ToArrayAsync(cancellationToken: cancellationToken);
        }
    }
}
