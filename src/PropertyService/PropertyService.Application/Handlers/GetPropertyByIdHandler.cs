using MediatR;
using PropertyService.Application.Queries;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Interfaces;

namespace PropertyService.Application.Handlers
{
    public class GetPropertyByIdHandler : IRequestHandler<GetPropertyByIdQuery, Property?>
    {
        private readonly IPropertyRepository _repository;

        public GetPropertyByIdHandler(IPropertyRepository repository)
        {
            _repository = repository;
        }

        public async Task<Property?> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id, cancellationToken);
        }
    }
}
