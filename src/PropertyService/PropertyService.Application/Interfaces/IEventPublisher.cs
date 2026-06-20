namespace PropertyService.Application.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string topicOrQueue, T eventMessage, CancellationToken cancellationToken = default);
    }
}
