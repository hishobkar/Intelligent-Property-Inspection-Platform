using InspectionService.Domain.Enums;

namespace InspectionService.Domain.Entities
{
    public class Inspection
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public string PropertyReference { get; set; } = string.Empty;
        public string InspectorId { get; set; } = string.Empty;
        public string InspectorName { get; set; } = string.Empty;
        public InspectionType Type { get; set; }
        public InspectionStatus Status { get; set; }
        public InspectionPriority Priority { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Findings { get; set; } = string.Empty;
        public int OverallConditionScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<InspectionPhoto> Photos { get; set; } = new List<InspectionPhoto>();
    }
}
