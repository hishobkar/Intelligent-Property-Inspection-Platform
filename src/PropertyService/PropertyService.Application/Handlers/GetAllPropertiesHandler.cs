using MediatR;
using PropertyService.Application.Queries;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Interfaces;

namespace PropertyService.Application.Handlers
{
    public class GetAllPropertiesHandler : IRequestHandler<GetAllPropertiesQuery, IEnumerable<Property>>
    {
        private readonly IPropertyRepository _repository;

        public GetAllPropertiesHandler(IPropertyRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Property>> Handle(GetAllPropertiesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        }
    }
}
