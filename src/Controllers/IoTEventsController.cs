using CoreBusinessService.Data;
using CoreBusinessService.Models;
using CoreBusinessService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreBusinessService.Controllers;

[ApiController]
[Route("api/iot-events")]
public class IoTEventsController(
    AppDbContext db,
    IAlertRuleService alertRules,
    IDatabaseInitializer databaseInitializer,
    INotificationService notifications,
    AnalyticsMqttService analyticsMqtt,
    ILogger<IoTEventsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(IoTEvent request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received IoT event. DeviceId={DeviceId}, Location={Location}, Temperature={Temperature}",
            request.DeviceId,
            request.Location,
            request.Temperature);

        await databaseInitializer.EnsureReadyAsync(cancellationToken);

        request.EventTime = NormalizeEventTime(request.EventTime);
        if (request.Timestamp is not null)
        {
            request.EventTime = request.Timestamp.Value;
        }

        db.IoTEvents.Add(request);

        var alert = alertRules.Evaluate(request);
        if (alert is not null)
        {
            db.Alerts.Add(alert);
            logger.LogWarning(
                "Created alert from IoT event. Severity={Severity}, Source={Source}, Message={Message}",
                alert.Severity,
                alert.Source,
                alert.Message);
        }

        await db.SaveChangesAsync(cancellationToken);

        var notification = alert is not null
            ? await notifications.SendAlertNotificationAsync(alert, cancellationToken)
            : null;

        bool analyticsSent = false;
        string? analyticsError = null;

        if (alert is not null)
        {
            var analyticsPayload = new
            {
                event_id = $"alert-{alert.Id}",
                event_type = "core.alert.created",
                timestamp = DateTime.UtcNow.ToString("o"),
                severity = alert.Severity == "high" ? "critical" : alert.Severity,
                source_event_id = $"iot-event-{request.Id}"
            };

            var analyticsResult = await analyticsMqtt.PublishAlertAsync(
                analyticsPayload,
                cancellationToken);

            analyticsSent = analyticsResult.Sent;
            analyticsError = analyticsResult.Error;
        }

        return Created($"/api/iot-events/{request.Id}", new
        {
            data = request,
            alertCreated = alert is not null,
            alert,
            notificationSent = notification?.Sent ?? false,
            notificationError = notification?.Error,
            analyticsSent,
            analyticsError
        });
    }

    private static DateTime NormalizeEventTime(DateTime eventTime) =>
        eventTime == default ? DateTime.UtcNow : eventTime;
}
