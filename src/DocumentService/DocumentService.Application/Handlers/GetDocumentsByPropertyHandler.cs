using DocumentService.Application.Queries;
using DocumentService.Domain.Entities;
using DocumentService.Domain.Interfaces;
using MediatR;

namespace DocumentService.Application.Handlers
{
    public class GetDocumentsByPropertyHandler : IRequestHandler<GetDocumentsByPropertyQuery, IEnumerable<Document>>
    {
        private readonly IDocumentRepository _repository;

        public GetDocumentsByPropertyHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Document>> Handle(GetDocumentsByPropertyQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetByPropertyIdAsync(request.PropertyId, cancellationToken);
        }
    }
}
