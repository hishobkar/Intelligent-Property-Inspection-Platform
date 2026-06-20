using DocumentService.Domain.Entities;
using MediatR;

namespace DocumentService.Application.Queries
{
    public class GetDocumentByIdQuery : IRequest<Document?>
    {
        public Guid Id { get; set; }
    }
}
