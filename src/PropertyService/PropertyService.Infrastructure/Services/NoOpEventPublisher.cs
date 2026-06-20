using Microsoft.Extensions.Logging;
using PropertyService.Application.Interfaces;

namespace PropertyService.Infrastructure.Services
{
    public class NoOpEventPublisher : IEventPublisher
    {
        private readonly ILogger<NoOpEventPublisher> _logger;

        public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync<T>(string topicOrQueue, T eventMessage, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Event published (no-op) to {Topic}: {EventType}", topicOrQueue, typeof(T).Name);
            return Task.CompletedTask;
        }
    }
}
