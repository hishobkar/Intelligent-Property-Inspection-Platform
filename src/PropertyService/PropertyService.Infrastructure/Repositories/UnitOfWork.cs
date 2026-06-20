using PropertyService.Domain.Interfaces;
using PropertyService.Infrastructure.Data;

namespace PropertyService.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PropertyDbContext _context;

        public UnitOfWork(PropertyDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
