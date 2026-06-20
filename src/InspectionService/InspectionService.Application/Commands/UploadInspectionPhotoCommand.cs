using InspectionService.Domain.Entities;
using MediatR;

namespace InspectionService.Application.Commands
{
    public class UploadInspectionPhotoCommand : IRequest<InspectionPhoto>
    {
        public Guid InspectionId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "image/jpeg";
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public string Description { get; set; } = string.Empty;
    }
}
