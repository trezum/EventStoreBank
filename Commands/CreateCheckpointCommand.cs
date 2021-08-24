using Model;
using System.Threading;
using System.Threading.Tasks;

namespace Commands
{
    public class CreateCheckpointCommand
    {
        private readonly BankContext _context;

        public CreateCheckpointCommand(BankContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync(ulong commitPosition, ulong preparePosition, CancellationToken cancellationToken)
        {
            await _context.Checkpoints.AddAsync(new Checkpoint()
            {
                Id = 1,
                CommitPosition = commitPosition,
                PreparePosition = preparePosition
            }, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
