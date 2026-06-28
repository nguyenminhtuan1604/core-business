using CoreBusinessService.Models;

namespace CoreBusinessService.Services;

public interface INotificationService
{
    Task<NotificationResult> SendAlertNotificationAsync(Alert alert, CancellationToken cancellationToken);
}
