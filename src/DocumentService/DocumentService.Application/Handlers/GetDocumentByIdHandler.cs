using DocumentService.Application.Queries;
using DocumentService.Domain.Entities;
using DocumentService.Domain.Interfaces;
using MediatR;

namespace DocumentService.Application.Handlers
{
    public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, Document?>
    {
        private readonly IDocumentRepository _repository;

        public GetDocumentByIdHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        public Task<Document?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetByIdAsync(request.Id, cancellationToken);
        }
    }
}
