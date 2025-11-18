using CreditEngineWorker;
using CreditEngineWorker.Configuration;
using CreditEngineWorker.Services;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System.Diagnostics;

var isService = !(Debugger.IsAttached || args.Contains("--console"));
var builder = Host.CreateApplicationBuilder(args);

// Configurar como serviço do Windows se necessário
if (isService)
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "LMS-CreditEngine-Worker";
    });
}

// Configurar injeção de dependência

builder.Services.Configure<MessageSettings>(builder.Configuration.GetSection("MessageSettings"));
builder.Services.Configure<CreditEngineApiSettings>(builder.Configuration.GetSection("CreditEngineApi"));
builder.Services.Configure<LetMeSeeApiSettings>(builder.Configuration.GetSection("LetMeSeeApiSettings"));

// Registrar serviços
builder.Services.AddHttpClient<IApiService, ApiService>();
builder.Services.AddScoped<ICreditEngineService, CreditEngineService>();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddHostedService<Worker>();

// Configurar logging com Serilog
var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "credit-engine-worker.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    .MinimumLevel.Override("RabbitMQ.Client", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .Enrich.WithExceptionDetails()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        logFilePath,
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// Configurar opções do host
builder.Services.Configure<HostOptions>(options =>
{
    options.ServicesStartConcurrently = false;
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

try
{
    Log.Information("Iniciando LMS Credit Engine Worker...");
    Log.Information("Ambiente: {Environment}", builder.Environment.EnvironmentName);
    Log.Information("Versão: {Version}", typeof(Program).Assembly.GetName().Version);

    var host = builder.Build();
    var logger = host.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Aplicação iniciada com sucesso!");
    logger.LogInformation("Worker configurado para processar fila: {Queue}",
        builder.Configuration["MessageSettings:Queue"]);

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha crítica ao iniciar o LMS Credit Engine Worker");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
