using CoreBusinessService.Data;
using CoreBusinessService.Models;
using CoreBusinessService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreBusinessService.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController(
    AppDbContext db,
    IDatabaseInitializer databaseInitializer,
    INotificationService notifications,
    ILogger<AlertsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        await databaseInitializer.EnsureReadyAsync(HttpContext.RequestAborted);

        var alerts = await db.Alerts
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received manual alert request. Severity={Severity}, Source={Source}",
            request.Severity,
            request.Source);

        await databaseInitializer.EnsureReadyAsync(cancellationToken);

        var alert = new Alert
        {
            Severity = request.Severity,
            Source = request.Source,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow
        };

        db.Alerts.Add(alert);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogWarning(
            "Created manual alert. AlertId={AlertId}, Severity={Severity}, Source={Source}, Message={Message}",
            alert.Id,
            alert.Severity,
            alert.Source,
            alert.Message);

        var notification = await notifications.SendAlertNotificationAsync(alert, cancellationToken);

        return Created($"/api/alerts/{alert.Id}", new
        {
            alertCreated = true,
            alert,
            notificationSent = notification.Sent,
            notificationError = notification.Error
        });
    }
}
