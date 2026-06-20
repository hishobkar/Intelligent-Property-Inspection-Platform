using InspectionService.Application.Queries;
using InspectionService.Domain.Entities;
using InspectionService.Domain.Interfaces;
using MediatR;

namespace InspectionService.Application.Handlers
{
    public class GetInspectionsByPropertyHandler : IRequestHandler<GetInspectionsByPropertyQuery, IEnumerable<Inspection>>
    {
        private readonly IInspectionRepository _repository;

        public GetInspectionsByPropertyHandler(IInspectionRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Inspection>> Handle(GetInspectionsByPropertyQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetByPropertyIdAsync(request.PropertyId, cancellationToken);
        }
    }
}
