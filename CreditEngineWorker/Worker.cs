using CreditEngineWorker.Services;
using Microsoft.Extensions.Options;
using CreditEngineWorker.Configuration;

namespace CreditEngineWorker;

public class Worker : BackgroundService
{
    private readonly IRabbitMQService _rabbitMQService;
    private readonly ILogger<Worker> _logger;
    private readonly MessageSettings _messageSettings;

    public Worker(
        IRabbitMQService rabbitMQService,
        ILogger<Worker> logger,
        IOptions<MessageSettings> messageSettings)
    {
        _rabbitMQService = rabbitMQService;
        _logger = logger;
        _messageSettings = messageSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LMS Credit Engine Worker iniciado às {Time}", DateTimeOffset.Now);

        try
        {
            // Aguardar um pouco para garantir que todos os serviços estejam prontos
            await Task.Delay(2000, stoppingToken);

            // Iniciar consumer do RabbitMQ
            await _rabbitMQService.StartConsumingAsync();

            _logger.LogInformation("Worker configurado e pronto para processar mensagens da fila: {Queue}",
                _messageSettings.Queue);

            // Manter o worker rodando
            while (!stoppingToken.IsCancellationRequested)
            {
                // Verificar conexão periodicamente
                var isConnected = await _rabbitMQService.IsConnectedAsync();
                if (!isConnected)
                {
                    _logger.LogWarning("Conexão com RabbitMQ perdida. Tentando reconectar...");
                    try
                    {
                        await _rabbitMQService.StopConsumingAsync();
                        await Task.Delay(5000, stoppingToken);
                        await _rabbitMQService.StartConsumingAsync();
                        _logger.LogInformation("Reconexão com RabbitMQ bem-sucedida");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao reconectar com RabbitMQ");
                    }
                }

                await Task.Delay(10000, stoppingToken); // Verificar a cada 10 segundos
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico no Worker");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando LMS Credit Engine Worker...");

        try
        {
            await _rabbitMQService.StopConsumingAsync();
            _logger.LogInformation("Worker parado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar worker");
        }

        await base.StopAsync(cancellationToken);
    }
}
