using CreditEngineWorker.Models;

namespace CreditEngineWorker.Services;

public class CreditEngineService : ICreditEngineService
{
    private readonly IApiService _apiService;
    private readonly ILogger<CreditEngineService> _logger;

    public CreditEngineService(
        IApiService apiService,
        ILogger<CreditEngineService> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public async Task<QueueMessage> ProcessCreditRequestAsync(QueueMessage request)
    {
        try
        {
            request.CreditEngineStatus.Add(new CreditEngineStatus ("Iniciando processamento da solicitação de crédito",2,  request.Id));
            _logger.LogInformation("Processando solicitação de crédito para cliente {AnalysisRequestName}",
                request.AnalysisRequestName);

            // Validar a solicitação primeiro
            var isValid = await ValidateCreditRequestAsync(request);

            if (!isValid.isValid)
            {
                _logger.LogWarning("Solicitação de crédito inválida para cliente {AnalysisRequestName}",
                    request.AnalysisRequestName);

                // Marcar como processada mas com erro
                request.CreditEngineStatus.Add(new CreditEngineStatus (isValid.ErrorMessage,3, request.Id));
                return request;
            }
            // Buscar as regras de crédito para a política de crédito
            var creditPolicyRules = await _apiService.GetCreditPolicyRulesAsync((long)request.CreditPolicyRuleId);
            if (creditPolicyRules == null)
            {
                _logger.LogError("Erro ao buscar regras de crédito para a política de crédito {CreditPolicyRuleId}",
                    request.CreditPolicyRuleId);
                request.CreditEngineStatus.Add(new CreditEngineStatus ("Erro ao buscar regras de crédito para a política de crédito",3,request.Id ));
                return request;
            }
            if(request.AnalysisRequestId == null)
            {
                _logger.LogError("AnalysisRequestId é nulo para a solicitação de crédito {AnalysisRequestName}",
                    request.AnalysisRequestName);
                request.CreditEngineStatus.Add(new CreditEngineStatus("AnalysisRequestId é nulo para a solicitação de crédito", 3, request.Id));
                return request;
            }
            //Aqui envio o cara para a api de credit engine
            var sendToCreditEngine = new SendToCreditEngine
            {
                RequestId = new List<long> { (long)request.AnalysisRequestId },
                SearchedItem = creditPolicyRules.ReportId ?? 0,
                CreditPolicyId = request.CreditPolicyId,
                CreditPolicyPfId = request.CreditPolicyPfId,
                Username = null,
                Password = null,
                Features = null,
                UseOnlyInternalData = creditPolicyRules?.UseOnInternalData == null ? false : (bool)creditPolicyRules.UseOnInternalData
            };
            var response = await _apiService.SendToCreditEngineAsync(sendToCreditEngine);
            if (response == null)
            {
                _logger.LogError("Erro ao enviar requisição para o credit engine para o pedido de crédito {AnalysisRequestName}",
                    request.AnalysisRequestName);
                request.CreditEngineStatus.Add(new CreditEngineStatus ("Erro ao enviar requisição para o credit engine", 3, request.Id));
                return request;
            }
            request.CreditEngineStatus.Add(new CreditEngineStatus ("Motor Processado.", 4, request.Id));
            request.TaskFinishedAt = DateTime.UtcNow;
            return request;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar solicitações de crédito para cliente {AnalysisRequestName}",
                request.AnalysisRequestName);
            request.CreditEngineStatus.Add(new CreditEngineStatus (ex.Message, 3, request.Id));
            return request;
        }
    }

    public async Task<ErrorResponse> ValidateCreditRequestAsync(QueueMessage request)
    {
        try
        {
            _logger.LogDebug("Validando solicitação de crédito para cliente {AnalysisRequestName}",
                request.AnalysisRequestName);
            var analysisRequest = await _apiService.GetAnalysisRequestAsync((long)request.AnalysisRequestId);
            if (analysisRequest == null)
            {
                return new ErrorResponse { isValid = false, ErrorMessage = "Pedido de crédito não encontrado", ErrorMessageCode = "ANALYSIS_REQUEST_NOT_FOUND" };
            }
            if (analysisRequest.Document == null || analysisRequest.Document == string.Empty)
            {
                return new ErrorResponse { isValid = false, ErrorMessage = "Documento é obrigatório", ErrorMessageCode = "DOCUMENT_REQUIRED" };
            }
            if (analysisRequest.Document.Length == 11 && request.CreditPolicyPfId == null)
            {
                return new ErrorResponse { isValid = false, ErrorMessage = "Documento tem 11 digitos porém não foi informado o ID da política de crédito (Pessoa Física)", ErrorMessageCode = "CREDIT_POLICY_PF_ID_REQUIRED" };
            }
            if (analysisRequest.Document.Length == 14 && request.CreditPolicyId == null)
            {
                return new ErrorResponse { isValid = false, ErrorMessage = "Documento tem 14 digitos porém não foi informado o ID da política de crédito (Pessoa Jurídica)", ErrorMessageCode = "CREDIT_POLICY_ID_REQUIRED" };
            }
            return new ErrorResponse { isValid = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar solicitação de crédito para cliente {AnalysisRequestId}",
                request.AnalysisRequestId);
            return new ErrorResponse { isValid = false, ErrorMessage = ex.Message, ErrorMessageCode = "VALIDATION_ERROR" };
        }
    }
}
