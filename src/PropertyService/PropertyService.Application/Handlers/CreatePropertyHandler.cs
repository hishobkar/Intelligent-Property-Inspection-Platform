using MediatR;
using Microsoft.Extensions.Logging;
using PropertyService.Application.Commands;
using PropertyService.Application.Interfaces;
using PropertyService.Domain.Entities;
using PropertyService.Domain.Enums;
using PropertyService.Domain.Events;
using PropertyService.Domain.Interfaces;

namespace PropertyService.Application.Handlers
{
    public class CreatePropertyHandler : IRequestHandler<CreatePropertyCommand, Property>
    {
        private readonly IPropertyRepository _repository;
        private readonly ILogger<CreatePropertyHandler> _logger;
        private readonly IEventPublisher _eventPublisher;

        public CreatePropertyHandler(
            IPropertyRepository repository,
            ILogger<CreatePropertyHandler> logger,
            IEventPublisher eventPublisher)
        {
            _repository = repository;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        public async Task<Property> Handle(CreatePropertyCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating new property at {Address}", request.Address);

            var property = new Property
            {
                Id = Guid.NewGuid(),
                PropertyReference = $"PROP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Address = request.Address,
                City = request.City,
                State = request.State,
                PostalCode = request.PostalCode,
                Country = request.Country,
                Type = request.Type,
                YearBuilt = request.YearBuilt,
                SquareFootage = request.SquareFootage,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                EstimatedValue = request.EstimatedValue,
                OwnerId = request.OwnerId,
                Status = PropertyStatus.Available,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.AddAsync(property, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            var evt = new PropertyCreatedEvent
            {
                PropertyId = property.Id,
                PropertyReference = property.PropertyReference,
                Address = property.Address,
                OwnerId = property.OwnerId,
                EstimatedValue = property.EstimatedValue,
                CreatedAt = property.CreatedAt
            };

            await _eventPublisher.PublishAsync("property-events", evt, cancellationToken);

            _logger.LogInformation("Property created with ID: {PropertyId}", property.Id);
            return property;
        }
    }
}
