using Model;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Queries
{
    public class GetCheckpointQuery
    {
        private readonly BankContext _context;

        public GetCheckpointQuery(BankContext context)
        {
            _context = context;
        }
        public async Task<Checkpoint> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await _context.Checkpoints.FirstOrDefaultAsync(c => c.Id == 1, cancellationToken);
        }
    }
}
