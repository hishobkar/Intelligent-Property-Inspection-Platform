using MediatR;

namespace DocumentService.Application.Commands
{
    public class DownloadDocumentCommand : IRequest<DocumentDownloadResult?>
    {
        public Guid DocumentId { get; set; }
    }

    public class DocumentDownloadResult
    {
        public Stream Stream { get; set; } = Stream.Null;
        public string ContentType { get; set; } = "application/octet-stream";
        public string FileName { get; set; } = "document";
    }
}
