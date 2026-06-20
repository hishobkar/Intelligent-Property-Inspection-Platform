using DocumentService.Domain.Enums;

namespace DocumentService.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public Guid PropertyId { get; set; }
        public Guid? InspectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public DocumentFormat Format { get; set; }
        public ReportStatus Status { get; set; }
        public string ContentText { get; set; } = string.Empty;
        public string BlobUrl { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string GeneratedBy { get; set; } = "System";
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
