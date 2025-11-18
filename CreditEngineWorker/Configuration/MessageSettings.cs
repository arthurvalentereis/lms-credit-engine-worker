namespace CreditEngineWorker.Configuration;

public class MessageSettings
{
    public string Hostname { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
}
