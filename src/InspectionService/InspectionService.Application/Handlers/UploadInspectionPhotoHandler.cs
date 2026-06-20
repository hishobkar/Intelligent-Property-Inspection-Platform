using InspectionService.Application.Commands;
using InspectionService.Domain.Entities;
using InspectionService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InspectionService.Application.Handlers
{
    public class UploadInspectionPhotoHandler : IRequestHandler<UploadInspectionPhotoCommand, InspectionPhoto>
    {
        private readonly IInspectionRepository _repository;
        private readonly ILogger<UploadInspectionPhotoHandler> _logger;

        public UploadInspectionPhotoHandler(IInspectionRepository repository, ILogger<UploadInspectionPhotoHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<InspectionPhoto> Handle(UploadInspectionPhotoCommand request, CancellationToken cancellationToken)
        {
            var inspection = await _repository.GetByIdAsync(request.InspectionId, cancellationToken)
                ?? throw new InvalidOperationException($"Inspection {request.InspectionId} not found");

            var photo = new InspectionPhoto
            {
                Id = Guid.NewGuid(),
                InspectionId = request.InspectionId,
                FileName = request.FileName.Length > 0 ? request.FileName : "photo.jpg",
                ContentType = request.ContentType,
                FileSizeBytes = request.FileData.Length,
                Description = request.Description,
                BlobUrl = $"/photos/{request.InspectionId}/{Guid.NewGuid()}.jpg",
                UploadedAt = DateTime.UtcNow
            };

            await _repository.AddPhotoAsync(photo, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Photo uploaded for inspection {InspectionId}", request.InspectionId);
            return photo;
        }
    }
}
