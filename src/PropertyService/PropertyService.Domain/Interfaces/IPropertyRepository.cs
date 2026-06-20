using PropertyService.Domain.Entities;

namespace PropertyService.Domain.Interfaces
{
    public interface IPropertyRepository
    {
        Task<Property?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Property>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<Property>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default);
        Task AddAsync(Property property, CancellationToken cancellationToken = default);
        void Update(Property property);
        void Delete(Property property);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
