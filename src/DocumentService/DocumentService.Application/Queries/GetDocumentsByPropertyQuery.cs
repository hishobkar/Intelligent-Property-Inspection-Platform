using DocumentService.Domain.Entities;
using MediatR;

namespace DocumentService.Application.Queries
{
    public class GetDocumentsByPropertyQuery : IRequest<IEnumerable<Document>>
    {
        public Guid PropertyId { get; set; }
    }
}
