namespace CoreBusinessService.Models;

public class VisionEvent
{
    public int Id { get; set; }
    public string CameraId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "low";
    public string? Description { get; set; }
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
}
