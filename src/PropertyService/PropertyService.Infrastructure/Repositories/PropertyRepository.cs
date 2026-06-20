using Microsoft.EntityFrameworkCore;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Interfaces;
using PropertyService.Infrastructure.Data;

namespace PropertyService.Infrastructure.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly PropertyDbContext _context;

        public PropertyRepository(PropertyDbContext context)
        {
            _context = context;
        }

        public async Task<Property?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Property>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.Properties
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Property>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
        {
            return await _context.Properties
                .Where(p => p.OwnerId == ownerId && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Property property, CancellationToken cancellationToken = default)
        {
            await _context.Properties.AddAsync(property, cancellationToken);
        }

        public void Update(Property property)
        {
            _context.Properties.Update(property);
        }

        public void Delete(Property property)
        {
            property.IsActive = false;
            property.UpdatedAt = DateTime.UtcNow;
            _context.Properties.Update(property);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Properties.AnyAsync(p => p.Id == id && p.IsActive, cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
