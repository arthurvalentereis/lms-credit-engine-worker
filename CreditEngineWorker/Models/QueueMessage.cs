using System.Text.Json.Serialization;

namespace CreditEngineWorker.Models;

public class QueueMessage
{
    [JsonPropertyName("CreditPolicyId")]
    public long? CreditPolicyId { get; set; }

    [JsonPropertyName("CreditPolicyPfId")]
    public long? CreditPolicyPfId { get; set; }

    [JsonPropertyName("AnalysisRequestId")]
    public long? AnalysisRequestId { get; set; }

    [JsonPropertyName("CreditPolicyRuleId")]
    public long? CreditPolicyRuleId { get; set; }

    [JsonPropertyName("AnalysisRequestName")]
    public string? AnalysisRequestName { get; set; } = string.Empty;

    [JsonPropertyName("UserId")]
    public long? UserId { get; set; }

    [JsonPropertyName("UserGroupId")]
    public long? UserGroupId { get; set; }

    [JsonPropertyName("TaskStartedAt")]
    public DateTime? TaskStartedAt { get; set; }

    [JsonPropertyName("TaskFinishedAt")]
    public DateTime? TaskFinishedAt { get; set; }

    [JsonPropertyName("CreditEngineStatus")]
    public List<CreditEngineStatus> CreditEngineStatus { get; set; } = new();

    [JsonPropertyName("Id")]
    public long Id { get; set; }

    [JsonPropertyName("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("Status")]
    public bool Status { get; set; }

}

public class CreditEngineStatus
{

    [JsonPropertyName("Data")]
    public DateTime Data { get; set; }
    [JsonPropertyName("Message")]
    public string? Message { get; set; }
    [JsonPropertyName("TaskStatusId")]
    public long TaskStatusId { get; set; }
    [JsonPropertyName("TaskCreditEngineSenderId")]
    public long TaskCreditEngineSenderId { get; set; }
    [JsonPropertyName("Id")]
    public long Id { get; set; }
    [JsonPropertyName("CreatedAt")]
    public DateTime CreatedAt { get; set; } 

    [JsonPropertyName("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } 

    [JsonPropertyName("Status")]
    public bool Status { get; set; }
    public CreditEngineStatus()
    {
    }
    public CreditEngineStatus(string? message, long taskStatusId, long taskCreditEngineSenderId)
    {
        Data = DateTime.UtcNow;
        Message = message;
        TaskStatusId = taskStatusId;
        TaskCreditEngineSenderId = taskCreditEngineSenderId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Status = true;
    }
}
