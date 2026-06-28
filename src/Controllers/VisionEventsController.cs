using CoreBusinessService.Data;
using CoreBusinessService.Models;
using CoreBusinessService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreBusinessService.Controllers;

[ApiController]
[Route("api/vision-events")]
public class VisionEventsController(
    AppDbContext db,
    IAlertRuleService alertRules,
    IDatabaseInitializer databaseInitializer,
    INotificationService notifications,
    ILogger<VisionEventsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(VisionEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received vision event. CameraId={CameraId}, Location={Location}, RiskLevel={RiskLevel}",
            request.CameraId,
            request.Location,
            request.RiskLevel);

        await databaseInitializer.EnsureReadyAsync(cancellationToken);
        request.EventTime = NormalizeEventTime(request.EventTime);
        db.VisionEvents.Add(request);

        var alert = alertRules.Evaluate(request);
        if (alert is not null)
        {
            db.Alerts.Add(alert);
            logger.LogWarning(
                "Created alert from vision event. Severity={Severity}, Source={Source}, Message={Message}",
                alert.Severity,
                alert.Source,
                alert.Message);
        }

        await db.SaveChangesAsync(cancellationToken);

        var notification = alert is not null
            ? await notifications.SendAlertNotificationAsync(alert, cancellationToken)
            : null;

        return Created($"/api/vision-events/{request.Id}", new
        {
            data = request,
            alertCreated = alert is not null,
            alert,
            notificationSent = notification?.Sent ?? false,
            notificationError = notification?.Error
        });
    }

    private static DateTime NormalizeEventTime(DateTime eventTime) =>
        eventTime == default ? DateTime.UtcNow : eventTime;
}
