namespace InspectionService.Domain.Entities
{
    public class InspectionPhoto
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string BlobUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }

        public Inspection? Inspection { get; set; }
    }
}
