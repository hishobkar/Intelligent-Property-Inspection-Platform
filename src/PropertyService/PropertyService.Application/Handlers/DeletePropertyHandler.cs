using MediatR;
using Microsoft.Extensions.Logging;
using PropertyService.Application.Commands;
using PropertyService.Domain.Interfaces;

namespace PropertyService.Application.Handlers
{
    public class DeletePropertyHandler : IRequestHandler<DeletePropertyCommand, bool>
    {
        private readonly IPropertyRepository _repository;
        private readonly ILogger<DeletePropertyHandler> _logger;

        public DeletePropertyHandler(IPropertyRepository repository, ILogger<DeletePropertyHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
        {
            var property = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (property == null)
            {
                _logger.LogWarning("Property {PropertyId} not found for deletion", request.Id);
                return false;
            }

            _repository.Delete(property);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Property {PropertyId} deleted", request.Id);
            return true;
        }
    }
}
