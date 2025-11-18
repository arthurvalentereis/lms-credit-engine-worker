public class SendToCreditEngine
{
    public List<long> RequestId { get; set; }
    public long SearchedItem { get; set; }
    public long? CreditPolicyId { get; set; }
    public long? CreditPolicyPfId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool? UseOnlyInternalData { get; set; }
    public List<Features>? Features { get; set; }
}

public class Features
{
    public long? Id { get; set; }
    public string? Name { get; set; }
}