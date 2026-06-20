using InspectionService.Domain.Entities;
using InspectionService.Domain.Enums;
using MediatR;

namespace InspectionService.Application.Commands
{
    public class CreateInspectionCommand : IRequest<Inspection>
    {
        public Guid PropertyId { get; set; }
        public string PropertyReference { get; set; } = string.Empty;
        public string InspectorId { get; set; } = string.Empty;
        public string InspectorName { get; set; } = string.Empty;
        public InspectionType Type { get; set; }
        public InspectionPriority Priority { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
