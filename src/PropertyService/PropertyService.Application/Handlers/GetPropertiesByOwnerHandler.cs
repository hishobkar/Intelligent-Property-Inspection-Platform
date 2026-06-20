using MediatR;
using PropertyService.Application.Queries;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Interfaces;

namespace PropertyService.Application.Handlers
{
    public class GetPropertiesByOwnerHandler : IRequestHandler<GetPropertiesByOwnerQuery, IEnumerable<Property>>
    {
        private readonly IPropertyRepository _repository;

        public GetPropertiesByOwnerHandler(IPropertyRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Property>> Handle(GetPropertiesByOwnerQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        }
    }
}
