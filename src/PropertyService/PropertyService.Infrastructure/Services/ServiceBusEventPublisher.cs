using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using PropertyService.Application.Interfaces;
using System.Text.Json;

namespace PropertyService.Infrastructure.Services
{
    public class ServiceBusEventPublisher : IEventPublisher
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<ServiceBusEventPublisher> _logger;

        public ServiceBusEventPublisher(ServiceBusClient client, ILogger<ServiceBusEventPublisher> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task PublishAsync<T>(string topicOrQueue, T eventMessage, CancellationToken cancellationToken = default)
        {
            try
            {
                var sender = _client.CreateSender(topicOrQueue);
                var body = JsonSerializer.Serialize(eventMessage);
                var message = new ServiceBusMessage(body)
                {
                    ContentType = "application/json",
                    Subject = typeof(T).Name
                };

                await sender.SendMessageAsync(message, cancellationToken);
                _logger.LogInformation("Event {EventType} published to {Topic}", typeof(T).Name, topicOrQueue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event {EventType} to {Topic}", typeof(T).Name, topicOrQueue);
                throw;
            }
        }
    }
}
