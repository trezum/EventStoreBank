using Microsoft.EntityFrameworkCore;
using Model;
using System.Threading;
using System.Threading.Tasks;

namespace Commands
{
    // The main reasons for this abstract class are to make sure to always update
    // a commit- and prepare position when handeling an event
    // and to keep that update and the update of the database in the same transaction
    // https://docs.microsoft.com/en-us/ef/core/saving/transactions
    public abstract class CommandBase<T> : ICommandInterface
    {
        protected readonly BankContext _context;

        public CommandBase(BankContext context)
        {
            _context = context;
        }
        // In this method the nessasary changes to dbcontext should be made.
        // Do not save changes in this method
        public abstract Task DBChanges(T model, CancellationToken cancellationToken);
        public async Task ExecuteAsync(T model, ulong commitPosition, ulong preparePosition, CancellationToken cancellationToken)
        {
            var checkpoint = await _context.Checkpoints.FirstOrDefaultAsync(c => c.Id == 1, cancellationToken);
            await DBChanges(model, cancellationToken);
            checkpoint.CommitPosition = commitPosition;
            checkpoint.PreparePosition = preparePosition;
            await _context.SaveChangesAsync();
        }
    }
}
