using CreditEngineWorker.Models;

namespace CreditEngineWorker.Services;

public interface IApiService
{
    Task<ResponseAnalysisRequest> GetAnalysisRequestAsync(long analysisRequestId);
    Task<QueueMessage> UpdateCreditEngineStatusAsync(QueueMessage request);
    Task<ResponseAnalysisRequest> SendToCreditEngineAsync(SendToCreditEngine request);
    Task<CreditPolicyRules> GetCreditPolicyRulesAsync(long creditPolicyId);

}
