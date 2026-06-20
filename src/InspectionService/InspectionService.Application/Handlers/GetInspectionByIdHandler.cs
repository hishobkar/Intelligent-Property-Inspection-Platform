using InspectionService.Application.Queries;
using InspectionService.Domain.Entities;
using InspectionService.Domain.Interfaces;
using MediatR;

namespace InspectionService.Application.Handlers
{
    public class GetInspectionByIdHandler : IRequestHandler<GetInspectionByIdQuery, Inspection?>
    {
        private readonly IInspectionRepository _repository;

        public GetInspectionByIdHandler(IInspectionRepository repository)
        {
            _repository = repository;
        }

        public Task<Inspection?> Handle(GetInspectionByIdQuery request, CancellationToken cancellationToken)
        {
            return _repository.GetByIdAsync(request.Id, cancellationToken);
        }
    }
}
