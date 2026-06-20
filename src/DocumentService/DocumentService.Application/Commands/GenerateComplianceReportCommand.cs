using MediatR;
using DocumentService.Domain.Entities;

namespace DocumentService.Application.Commands
{
    public class GenerateComplianceReportCommand : IRequest<Document>
    {
        public Guid PropertyId { get; set; }
        public Guid? InspectionId { get; set; }
        public string ReportType { get; set; } = "Compliance";
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}