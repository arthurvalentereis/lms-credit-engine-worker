using CreditEngineWorker.Models;

namespace CreditEngineWorker.Services;

public interface IRabbitMQService
{
    Task StartConsumingAsync();
    Task StopConsumingAsync();
    Task PublishMessageAsync<T>(T message, string routingKey);
    Task<bool> IsConnectedAsync();
}
