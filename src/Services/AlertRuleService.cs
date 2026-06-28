using CoreBusinessService.Models;

namespace CoreBusinessService.Services;

public class AlertRuleService : IAlertRuleService
{
    public Alert? Evaluate(IoTEvent iotEvent)
    {
        if (iotEvent.Temperature <= 35)
        {
            return null;
        }

        return new Alert
        {
            Severity = "high",
            Source = "iot",
            Message = $"Temperature is above threshold at {iotEvent.Location}: {iotEvent.Temperature}C",
            CreatedAt = DateTime.UtcNow
        };
    }

    public Alert? Evaluate(AccessEvent accessEvent)
    {
        if (string.Equals(accessEvent.Result, "denied", StringComparison.OrdinalIgnoreCase))
        {
            return new Alert
            {
                Severity = "medium",
                Source = "access",
                Message = $"Access denied for user {accessEvent.UserId} at {accessEvent.DoorId}",
                CreatedAt = DateTime.UtcNow
            };
        }

        var hour = accessEvent.EventTime.Hour;
        var outsideOfficeHours = hour >= 22 || hour < 5;

        if (!outsideOfficeHours)
        {
            return null;
        }

        return new Alert
        {
            Severity = "medium",
            Source = "access",
            Message = $"Access event outside allowed time for user {accessEvent.UserId} at {accessEvent.DoorId}",
            CreatedAt = DateTime.UtcNow
        };
    }

    public Alert? Evaluate(VisionEvent visionEvent)
    {
        if (!string.Equals(visionEvent.RiskLevel, "high", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new Alert
        {
            Severity = "high",
            Source = "vision",
            Message = $"High risk vision event detected at {visionEvent.Location}",
            CreatedAt = DateTime.UtcNow
        };
    }
}
