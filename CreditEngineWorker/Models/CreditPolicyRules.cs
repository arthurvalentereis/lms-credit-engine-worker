using System.Text.Json.Serialization;

namespace CreditEngineWorker.Models;

public class CreditPolicyRules
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("externalCreditPolicyMetricsEntepriseId")]
    public long? ExternalCreditPolicyMetricsEntepriseId { get; set; }

    [JsonPropertyName("externalCreditPolicyMetricsPersonId")]
    public long? ExternalCreditPolicyMetricsPersonId { get; set; }

    [JsonPropertyName("customerType")]
    public long? CustomerType { get; set; }

    [JsonPropertyName("customerStatus")]
    public long? CustomerStatus { get; set; }

    [JsonPropertyName("hasInvoice")]
    public bool HasInvoice { get; set; }

    [JsonPropertyName("minCreditAmount")]
    public decimal MinCreditAmount { get; set; }

    [JsonPropertyName("maxCreditAmount")]
    public decimal MaxCreditAmount { get; set; }

    [JsonPropertyName("cnae")]
    public string? Cnae { get; set; }

    [JsonPropertyName("useOnInternalData")]
    public bool UseOnInternalData { get; set; }

    [JsonPropertyName("reportId")]
    public long? ReportId { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }
}
