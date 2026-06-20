using DocumentService.Domain.Entities;

namespace DocumentService.Domain.Interfaces
{
    public interface IDocumentRepository
    {
        Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Document>> GetByPropertyIdAsync(Guid propertyId, CancellationToken cancellationToken = default);
        Task AddAsync(Document document, CancellationToken cancellationToken = default);
        void Update(Document document);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
