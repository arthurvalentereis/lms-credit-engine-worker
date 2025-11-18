using CreditEngineWorker.Models;

namespace CreditEngineWorker.Services;

public interface ICreditEngineService
{
    Task<QueueMessage> ProcessCreditRequestAsync(QueueMessage request);
    Task<ErrorResponse> ValidateCreditRequestAsync(QueueMessage request);
}
