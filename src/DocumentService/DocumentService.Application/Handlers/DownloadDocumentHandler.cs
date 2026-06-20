using DocumentService.Application.Commands;
using DocumentService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DocumentService.Application.Handlers
{
    public class DownloadDocumentHandler : IRequestHandler<DownloadDocumentCommand, DocumentDownloadResult?>
    {
        private readonly IDocumentRepository _repository;
        private readonly ILogger<DownloadDocumentHandler> _logger;

        public DownloadDocumentHandler(IDocumentRepository repository, ILogger<DownloadDocumentHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<DocumentDownloadResult?> Handle(DownloadDocumentCommand request, CancellationToken cancellationToken)
        {
            var document = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);
            if (document == null)
            {
                _logger.LogWarning("Document {DocumentId} not found", request.DocumentId);
                return null;
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(document.ContentText);
            return new DocumentDownloadResult
            {
                Stream = new MemoryStream(bytes),
                ContentType = "text/html; charset=utf-8",
                FileName = $"{document.Title.Replace(" ", "_")}.html"
            };
        }
    }
}
