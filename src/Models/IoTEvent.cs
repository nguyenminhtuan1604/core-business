namespace CoreBusinessService.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class IoTEvent
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public decimal? Humidity { get; set; }
    public bool? Motion { get; set; }
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public DateTime? Timestamp { get; set; }
}
