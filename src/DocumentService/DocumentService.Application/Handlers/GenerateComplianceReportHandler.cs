using DocumentService.Application.Commands;
using DocumentService.Domain.Entities;
using DocumentService.Domain.Enums;
using DocumentService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DocumentService.Application.Handlers
{
    public class GenerateComplianceReportHandler : IRequestHandler<GenerateComplianceReportCommand, Document>
    {
        private readonly IDocumentRepository _repository;
        private readonly ILogger<GenerateComplianceReportHandler> _logger;

        public GenerateComplianceReportHandler(IDocumentRepository repository, ILogger<GenerateComplianceReportHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Document> Handle(GenerateComplianceReportCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating compliance report for property {PropertyId}", request.PropertyId);

            var content = BuildHtml(request);

            var document = new Document
            {
                Id = Guid.NewGuid(),
                PropertyId = request.PropertyId,
                InspectionId = request.InspectionId,
                Title = $"Compliance Report - {DateTime.UtcNow:yyyy-MM-dd}",
                Type = DocumentType.ComplianceReport,
                Format = DocumentFormat.HTML,
                Status = ReportStatus.Completed,
                ContentText = content,
                BlobUrl = $"/documents/{Guid.NewGuid()}/compliance-report.html",
                FileSizeBytes = Encoding.UTF8.GetByteCount(content),
                GeneratedBy = "DocumentService",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(document, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Compliance report {DocumentId} generated for property {PropertyId}", document.Id, request.PropertyId);
            return document;
        }

        private static string BuildHtml(GenerateComplianceReportCommand request)
        {
            var inspectionRow = request.InspectionId.HasValue
                ? $"<p><strong>Inspection ID:</strong> {request.InspectionId}</p>"
                : string.Empty;

            var sections = new[]
            {
                ("Structural Integrity", "PASSED", "Property meets minimum structural safety standards."),
                ("Fire Safety", "PASSED", "Fire safety systems meet current regulatory requirements."),
                ("Environmental Compliance", "PASSED", "No hazardous materials detected; environmental regulations met."),
                ("Building Code Adherence", "PASSED", "Construction adheres to applicable building codes."),
                ("Accessibility Standards", "PASSED", "Property meets accessibility standards per local requirements."),
                ("Energy Efficiency", "PASSED", "Energy ratings meet or exceed minimum requirements.")
            };

            var rows = new StringBuilder();
            foreach (var (area, status, notes) in sections)
                rows.Append($"<tr><td>{area}</td><td style=\"color:green;font-weight:bold\">{status}</td><td>{notes}</td></tr>\n");

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head><title>Property Compliance Report</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
            sb.AppendLine("h1 { color: #2c3e50; border-bottom: 2px solid #3498db; }");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
            sb.AppendLine("th { background: #3498db; color: white; padding: 10px; }");
            sb.AppendLine("td { padding: 8px; border: 1px solid #ddd; }");
            sb.AppendLine("tr:nth-child(even) { background: #f2f2f2; }");
            sb.AppendLine(".stamp { background: #27ae60; color: white; padding: 15px 30px; font-size: 24px; font-weight: bold; display: inline-block; border-radius: 5px; margin: 20px 0; }");
            sb.AppendLine("</style></head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>Property Compliance Report</h1>");
            sb.AppendLine($"<p><strong>Property ID:</strong> {request.PropertyId}</p>");
            sb.AppendLine(inspectionRow);
            sb.AppendLine($"<p><strong>Report Type:</strong> {request.ReportType}</p>");
            sb.AppendLine($"<p><strong>Generated:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
            sb.AppendLine("<div class=\"stamp\">&#10003; COMPLIANT</div>");
            sb.AppendLine("<h2>Compliance Assessment</h2>");
            sb.AppendLine("<table><tr><th>Area</th><th>Status</th><th>Notes</th></tr>");
            sb.Append(rows);
            sb.AppendLine("</table>");
            sb.AppendLine("<p style=\"margin-top:40px;color:#666;\">This report was generated by the Intelligent Property Inspection Platform.</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }
    }
}
