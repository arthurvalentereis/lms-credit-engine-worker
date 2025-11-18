using CreditEngineWorker.Configuration;
using CreditEngineWorker.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CreditEngineWorker.Services;

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private readonly MessageSettings _messageSettings;
    private readonly ICreditEngineService _creditEngineService;
    private readonly IApiService _apiService;
    private readonly ILogger<RabbitMQService> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private EventingBasicConsumer? _consumer;
    private bool _disposed = false;

    public RabbitMQService(
        IOptions<MessageSettings> messageSettings,
        ICreditEngineService creditEngineService,
        IApiService apiService,
        ILogger<RabbitMQService> logger)
    {
        _messageSettings = messageSettings.Value;
        _creditEngineService = creditEngineService;
        _apiService = apiService;
        _logger = logger;
    }

    public async Task StartConsumingAsync()
    {
        try
        {
            await ConnectAsync();
            await SetupQueueAsync();
            await StartConsumerAsync();

            _logger.LogInformation("RabbitMQ consumer iniciado com sucesso. Queue: {Queue}", _messageSettings.Queue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar consumer do RabbitMQ");
            throw;
        }
    }

    public async Task StopConsumingAsync()
    {
        try
        {
            if (_channel?.IsOpen == true)
            {
                await Task.Run(() => _channel.Close());
            }

            if (_connection?.IsOpen == true)
            {
                await Task.Run(() => _connection.Close());
            }

            _logger.LogInformation("RabbitMQ consumer parado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar consumer do RabbitMQ");
        }
    }

    public async Task PublishMessageAsync<T>(T message, string routingKey)
    {
        try
        {
            if (_channel?.IsOpen != true)
            {
                await ConnectAsync();
            }

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(
                exchange: _messageSettings.Exchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            _logger.LogDebug("Mensagem publicada com sucesso. RoutingKey: {RoutingKey}", routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem. RoutingKey: {RoutingKey}", routingKey);
            throw;
        }
    }

    public Task<bool> IsConnectedAsync()
    {
        return Task.FromResult(_connection?.IsOpen == true && _channel?.IsOpen == true);
    }

    private async Task ConnectAsync()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _messageSettings.Hostname,
                Port = _messageSettings.Port,
                UserName = _messageSettings.Username,
                Password = _messageSettings.Password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
            factory.Uri = new Uri(_messageSettings.Url);

            _connection = await Task.Run(() => factory.CreateConnection());
            _channel = _connection.CreateModel();

            _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar com RabbitMQ");
            throw;
        }
    }

    private Task SetupQueueAsync()
    {
        try
        {
            // Declarar exchange
            // _channel!.ExchangeDeclare(
            //     exchange: _messageSettings.Exchange,
            //     type: ExchangeType.Direct,
            //     durable: true,
            //     autoDelete: false);

            // Declarar queue
            _channel!.QueueDeclare(
                queue: _messageSettings.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Fazer bind da queue com o exchange
            // _channel.QueueBind(
            //     queue: _messageSettings.Queue,
            //     exchange: _messageSettings.Exchange,
            //     routingKey: _messageSettings.RoutingKey);

            // Configurar QoS
            // _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            _logger.LogInformation("Queue configurada com sucesso: {Queue}", _messageSettings.Queue);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar queue");
            throw;
        }
    }

    private Task StartConsumerAsync()
    {
        try
        {
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += async (model, ea) =>
            {
                await ProcessMessageAsync(ea);
            };

            _channel!.BasicConsume(
                queue: _messageSettings.Queue,
                autoAck: false,
                consumer: _consumer);

            _logger.LogInformation("Consumer iniciado com sucesso");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar consumer");
            throw;
        }
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
    {
        var deliveryTag = ea.DeliveryTag;
        var correlationId = ea.BasicProperties.CorrelationId;

        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("Mensagem recebida. CorrelationId: {CorrelationId}", correlationId);

            var queueMessage = JsonSerializer.Deserialize<QueueMessage>(message);
            if (queueMessage == null)
            {
                _logger.LogWarning("Mensagem inválida recebida. CorrelationId: {CorrelationId}", correlationId);
                _channel?.BasicAck(deliveryTag, false);
                return;
            }
            var response = await _creditEngineService.ProcessCreditRequestAsync(queueMessage);
            await _apiService.UpdateCreditEngineStatusAsync(response);
           _channel?.BasicAck(deliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem. CorrelationId: {CorrelationId}", correlationId);

            // Rejeitar mensagem
           _channel?.BasicNack(deliveryTag, false, false);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
