using MediatR;

namespace InspectionService.Application.Commands
{
    public class CompleteInspectionCommand : IRequest<bool>
    {
        public Guid InspectionId { get; set; }
        public string Findings { get; set; } = string.Empty;
        public int OverallConditionScore { get; set; }
    }
}
