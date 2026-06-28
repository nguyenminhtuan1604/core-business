using CoreBusinessService.Models;
using System.Net.Http.Json;

namespace CoreBusinessService.Services;

public class NotificationService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<NotificationResult> SendAlertNotificationAsync(Alert alert, CancellationToken cancellationToken)
    {
        var baseUrl = configuration["NOTIFICATION_SERVICE_URL"] ?? "http://localhost:8000";
        var endpoint = $"{baseUrl.TrimEnd('/')}/api/notifications";

        var payload = new
        {
            alert_id = alert.Id.ToString(),
            severity = alert.Severity,
            message = alert.Message,
            source = alert.Source,
            target = "security_team"
        };

        try
        {
            logger.LogInformation(
                "Sending notification to B7. Endpoint={Endpoint}, AlertId={AlertId}, Severity={Severity}, Source={Source}",
                endpoint,
                alert.Id,
                alert.Severity,
                alert.Source);

            using var response = await httpClient.PostAsJsonAsync(endpoint, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = $"B7 notification failed with HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                logger.LogError(
                    "Notification failed. AlertId={AlertId}, StatusCode={StatusCode}, Reason={Reason}",
                    alert.Id,
                    (int)response.StatusCode,
                    response.ReasonPhrase);

                return NotificationResult.Failure(error);
            }

            logger.LogInformation("Notification sent successfully. AlertId={AlertId}", alert.Id);
            return NotificationResult.Success();
        }
        catch (TaskCanceledException ex)
        {
            var error = $"B7 notification timeout after {httpClient.Timeout.TotalSeconds:0} seconds";
            logger.LogError(ex, "Notification timeout. AlertId={AlertId}", alert.Id);
            return NotificationResult.Failure(error);
        }
        catch (Exception ex)
        {
            var error = $"B7 notification error: {ex.Message}";
            logger.LogError(ex, "Notification error. AlertId={AlertId}", alert.Id);
            return NotificationResult.Failure(error);
        }
    }
}
