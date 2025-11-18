using System.Text.Json.Serialization;

namespace CreditEngineWorker.Models;

public class ResponseAnalysisRequest
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public long? UserGroupId { get; set; }
    public string? Name { get; set; }
    public string? Document { get; set; }
    public long? HeaderId { get; set; }
    public DateTime RequestedDate { get; set; }
    public decimal? RequestedAmount { get; set; }
    public string? Condition { get; set; }
    public string? Annotations { get; set; }
    public DateTime? ProcessFinishedAt { get; set; }
    public bool? Approved { get; set; }
    public string? ApprovedReason { get; set; }
    public bool? AutomaticallyResolved { get; set; }
    public long? AnalysisRequestCategoryId { get; set; }
    public string? AnalysisRequestCategoryName { get; set; }
    public string? AnalysisRequestStatusName { get; set; }
    public string? UserName { get; set; }
    public string? Notification { get; set; }
    public long? AnalysisRequestStatusId { get; set; }
    public long? UserCompanyId { get; set; }
    public string? SharedLink { get; set; }
    public decimal? CreditLimit { get; set; }
}

public class ErrorResponse
{
    public bool isValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorMessageCode { get; set; }
}