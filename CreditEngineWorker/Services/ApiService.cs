using CreditEngineWorker.Configuration;
using CreditEngineWorker.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace CreditEngineWorker.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly LetMeSeeApiSettings _letMeSeeApiSettings;
    private readonly ILogger<ApiService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiService(
        HttpClient httpClient,
        IOptions<LetMeSeeApiSettings> letMeSeeApiSettings,
        ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _letMeSeeApiSettings = letMeSeeApiSettings.Value;
        _logger = logger;

        // Configurar HttpClient
        _httpClient.BaseAddress = new Uri(_letMeSeeApiSettings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "LMS-CreditEngine-Worker/1.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        // Configurar opções de serialização JSON
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }
    public async Task<ResponseAnalysisRequest> GetAnalysisRequestAsync(long analysisRequestId)
    {
        ResponseAnalysisRequest? response = null;
        _logger.LogInformation($"Buscando analysis request  {analysisRequestId}  Data : {DateTime.Now}");
        var result = await SendToApi($"workerIntegration/get-analysis-request-by-id?analysisRequestId={analysisRequestId}", "GET");
        if (result != "")
        {
            response = JsonConvert.DeserializeObject<ResponseAnalysisRequest>(result);
        }
        return response ;
    }
    public async Task<QueueMessage> UpdateCreditEngineStatusAsync(QueueMessage request)
    {
        QueueMessage? response = null;
        _logger.LogInformation($"Atualizando status do credit engine para o pedido de crédito {request.AnalysisRequestId}  Data : {DateTime.Now}");
        var result = await SendToApi($"workerIntegration/update-task-credit-engine-status", "PUT", JsonConvert.SerializeObject(request));
        if (result != "")
        {
            response = JsonConvert.DeserializeObject<QueueMessage>(result);
        }
        return response;
    }
    public async Task<ResponseAnalysisRequest> SendToCreditEngineAsync(SendToCreditEngine request)
    {
        ResponseAnalysisRequest? response = null;
        _logger.LogInformation($"Enviando requisição para o credit engine para o pedido de crédito {request.RequestId.FirstOrDefault()}  Data : {DateTime.Now}");
        var result = await SendToApi($"workerIntegration/process-engine", "POST", JsonConvert.SerializeObject(request));
        if (result != "")
        {
            response = JsonConvert.DeserializeObject<ResponseAnalysisRequest>(result);
        }
        return response ;
    }
    public async Task<CreditPolicyRules> GetCreditPolicyRulesAsync(long creditPolicyId)
    {
        CreditPolicyRules? response = null;
        _logger.LogInformation($"Buscando regras de crédito para a política de crédito {creditPolicyId}  Data : {DateTime.Now}");
        var result = await SendToApi($"workerIntegration/get-info-rules-by-id?rulesId={creditPolicyId}", "GET");
        if (result != "")
        {
            response = JsonConvert.DeserializeObject<CreditPolicyRules>(result);
        }
        return response;
    }

    private async Task<string> SendToApi(string endpoint, string metodo, string? jsonBody = null)
    {
        string url = $"{_letMeSeeApiSettings.BaseUrl}{endpoint}";

        HttpRequestMessage request = new HttpRequestMessage
        {
            RequestUri = new Uri(url),
            Method = new HttpMethod(metodo)
        };

        if (jsonBody != null)
        {
            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        }

        try
        {
            _logger.LogDebug("Enviando requisição para: {Method} {Url}", metodo, url);
            if (jsonBody != null)
            {
                _logger.LogDebug("Body da requisição: {Body}", jsonBody);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Resposta da API: {StatusCode} - {Body}", response.StatusCode, responseBody);
                return responseBody;
            }
            else
            {
                _logger.LogError("Erro na API: {StatusCode} - {ReasonPhrase} - {Body}",
                    response.StatusCode, response.ReasonPhrase, responseBody);

                // Para erro 400, vamos logar mais detalhes
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogError("Detalhes do erro 400 - URL: {Url}, Method: {Method}, Body enviado: {Body}",
                        url, metodo, jsonBody ?? "null");
                }

                return "";
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de conexão HTTP ao enviar requisição para: {Url}", url);
            return "";
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout ao enviar requisição para: {Url}", url);
            return "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao enviar requisição para: {Url}", url);
            return "";
        }
    }
}
