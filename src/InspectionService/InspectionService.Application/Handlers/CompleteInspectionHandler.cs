using InspectionService.Application.Commands;
using InspectionService.Domain.Enums;
using InspectionService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InspectionService.Application.Handlers
{
    public class CompleteInspectionHandler : IRequestHandler<CompleteInspectionCommand, bool>
    {
        private readonly IInspectionRepository _repository;
        private readonly ILogger<CompleteInspectionHandler> _logger;

        public CompleteInspectionHandler(IInspectionRepository repository, ILogger<CompleteInspectionHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(CompleteInspectionCommand request, CancellationToken cancellationToken)
        {
            var inspection = await _repository.GetByIdAsync(request.InspectionId, cancellationToken);
            if (inspection == null)
            {
                _logger.LogWarning("Inspection {InspectionId} not found", request.InspectionId);
                return false;
            }

            inspection.Status = InspectionStatus.Completed;
            inspection.CompletedDate = DateTime.UtcNow;
            inspection.Findings = request.Findings;
            inspection.OverallConditionScore = request.OverallConditionScore;
            inspection.UpdatedAt = DateTime.UtcNow;

            _repository.Update(inspection);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inspection {InspectionId} completed with score {Score}", inspection.Id, inspection.OverallConditionScore);
            return true;
        }
    }
}
