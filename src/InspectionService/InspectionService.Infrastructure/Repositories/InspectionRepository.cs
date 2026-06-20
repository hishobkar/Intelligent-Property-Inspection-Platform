using InspectionService.Domain.Entities;
using InspectionService.Domain.Interfaces;
using InspectionService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InspectionService.Infrastructure.Repositories
{
    public class InspectionRepository : IInspectionRepository
    {
        private readonly InspectionDbContext _context;

        public InspectionRepository(InspectionDbContext context)
        {
            _context = context;
        }

        public async Task<Inspection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Inspections
                .Include(i => i.Photos)
                .FirstOrDefaultAsync(i => i.Id == id && i.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Inspection>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Inspections
                .Include(i => i.Photos)
                .Where(i => i.IsActive)
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Inspection>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default)
        {
            return await _context.Inspections
                .Include(i => i.Photos)
                .Where(i => i.PropertyId == propertyId && i.IsActive)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default)
        {
            await _context.Inspections.AddAsync(inspection, cancellationToken);
        }

        public void Update(Inspection inspection)
        {
            _context.Inspections.Update(inspection);
        }

        public async Task AddPhotoAsync(InspectionPhoto photo, CancellationToken cancellationToken = default)
        {
            await _context.InspectionPhotos.AddAsync(photo, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
