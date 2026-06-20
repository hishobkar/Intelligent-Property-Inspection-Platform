using MediatR;
using Microsoft.Extensions.Logging;
using PropertyService.Application.Commands;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Interfaces;

namespace PropertyService.Application.Handlers
{
    public class UpdatePropertyHandler : IRequestHandler<UpdatePropertyCommand, Property?>
    {
        private readonly IPropertyRepository _repository;
        private readonly ILogger<UpdatePropertyHandler> _logger;

        public UpdatePropertyHandler(IPropertyRepository repository, ILogger<UpdatePropertyHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Property?> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found for update", request.Id);
                return null;
            }

            property.Address = request.Address;
            property.City = request.City;
            property.State = request.State;
            property.PostalCode = request.PostalCode;
            property.Country = request.Country;
            property.Type = request.Type;
            property.YearBuilt = request.YearBuilt;
            property.SquareFootage = request.SquareFootage;
            property.Bedrooms = request.Bedrooms;
            property.Bathrooms = request.Bathrooms;
            property.EstimatedValue = request.EstimatedValue;
            property.Status = request.Status;
            property.UpdatedAt = DateTime.UtcNow;

            _repository.Update(property);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Property {PropertyId} updated successfully", property.Id);
            return property;
        }
    }
}
