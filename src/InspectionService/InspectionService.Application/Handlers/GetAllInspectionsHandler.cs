using InspectionService.Application.Queries;
using InspectionService.Domain.Entities;
using InspectionService.Domain.Interfaces;
using MediatR;

namespace InspectionService.Application.Handlers
{
    public class GetAllInspectionsHandler : IRequestHandler<GetAllInspectionsQuery, IEnumerable<Inspection>>
    {
        private readonly IInspectionRepository _repository;

        public GetAllInspectionsHandler(IInspectionRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Inspection>> Handle(GetAllInspectionsQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        }
    }
}
