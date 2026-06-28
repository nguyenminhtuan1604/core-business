using CoreBusinessService.Models;

namespace CoreBusinessService.Services;

public interface IAlertRuleService
{
    Alert? Evaluate(IoTEvent iotEvent);
    Alert? Evaluate(AccessEvent accessEvent);
    Alert? Evaluate(VisionEvent visionEvent);
}
