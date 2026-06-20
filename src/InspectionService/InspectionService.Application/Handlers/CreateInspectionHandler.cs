using InspectionService.Application.Commands;
using InspectionService.Domain.Entities;
using InspectionService.Domain.Enums;
using InspectionService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InspectionService.Application.Handlers
{
    public class CreateInspectionHandler : IRequestHandler<CreateInspectionCommand, Inspection>
    {
        private readonly IInspectionRepository _repository;
        private readonly ILogger<CreateInspectionHandler> _logger;

        public CreateInspectionHandler(IInspectionRepository repository, ILogger<CreateInspectionHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Inspection> Handle(CreateInspectionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating inspection for property {PropertyId}", request.PropertyId);

            var inspection = new Inspection
            {
                Id = Guid.NewGuid(),
                PropertyId = request.PropertyId,
                PropertyReference = request.PropertyReference,
                InspectorId = request.InspectorId,
                InspectorName = request.InspectorName,
                Type = request.Type,
                Priority = request.Priority,
                Status = InspectionStatus.Scheduled,
                ScheduledDate = request.ScheduledDate,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _repository.AddAsync(inspection, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inspection {InspectionId} created for property {PropertyId}", inspection.Id, inspection.PropertyId);
            return inspection;
        }
    }
}
