namespace CoreBusinessService.Models;

public class NotificationResult
{
    public bool Sent { get; set; }
    public string? Error { get; set; }

    public static NotificationResult Success() => new() { Sent = true };

    public static NotificationResult Failure(string error) => new()
    {
        Sent = false,
        Error = error
    };
}
