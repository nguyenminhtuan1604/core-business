using CoreBusinessService.Data;
using CoreBusinessService.Models;
using CoreBusinessService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoreBusinessService.Controllers;

[ApiController]
[Route("api/access-events")]
public class AccessEventsController(
    AppDbContext db,
    IAlertRuleService alertRules,
    IDatabaseInitializer databaseInitializer,
    INotificationService notifications,
    ILogger<AccessEventsController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(AccessEvent request, CancellationToken cancellationToken)
    {
        NormalizeRequest(request);

        logger.LogInformation(
            "Received access event. UserId={UserId}, DoorId={DoorId}, Result={Result}, Direction={Direction}, EventTime={EventTime}",
            request.UserId,
            request.DoorId,
            request.Result,
            request.Direction,
            request.EventTime);

        await databaseInitializer.EnsureReadyAsync(cancellationToken);
        db.AccessEvents.Add(request);

        var alert = alertRules.Evaluate(request);
        if (alert is not null)
        {
            db.Alerts.Add(alert);
            logger.LogWarning(
                "Created alert from access event. Severity={Severity}, Source={Source}, Message={Message}",
                alert.Severity,
                alert.Source,
                alert.Message);
        }

        await db.SaveChangesAsync(cancellationToken);

        var notification = alert is not null
            ? await notifications.SendAlertNotificationAsync(alert, cancellationToken)
            : null;

        return Created($"/api/access-events/{request.Id}", new
        {
            data = request,
            alertCreated = alert is not null,
            alert,
            notificationSent = notification?.Sent ?? false,
            notificationError = notification?.Error
        });
    }

    private static void NormalizeRequest(AccessEvent request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) && !string.IsNullOrWhiteSpace(request.CardId))
        {
            request.UserId = request.CardId;
        }

        if (string.IsNullOrWhiteSpace(request.DoorId) && !string.IsNullOrWhiteSpace(request.GateId))
        {
            request.DoorId = request.GateId;
        }

        if (string.IsNullOrWhiteSpace(request.Result))
        {
            request.Result = "granted";
        }

        if (request.Timestamp is not null)
        {
            request.EventTime = request.Timestamp.Value;
        }
        else if (request.EventTime == default)
        {
            request.EventTime = DateTime.UtcNow;
        }
    }
}
