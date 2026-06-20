using DocumentService.Domain.Entities;
using MediatR;

namespace DocumentService.Application.Commands
{
    public class GenerateInspectionReportCommand : IRequest<Document>
    {
        public Guid PropertyId { get; set; }
        public Guid InspectionId { get; set; }
        public string ReportType { get; set; } = "InspectionReport";
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
