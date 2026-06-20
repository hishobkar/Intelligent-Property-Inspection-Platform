using InspectionService.Domain.Entities;

namespace InspectionService.Domain.Interfaces
{
    public interface IInspectionRepository
    {
        Task<Inspection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Inspection>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<Inspection>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default);
        Task AddAsync(Inspection inspection, CancellationToken cancellationToken = default);
        void Update(Inspection inspection);
        Task AddPhotoAsync(InspectionPhoto photo, CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
