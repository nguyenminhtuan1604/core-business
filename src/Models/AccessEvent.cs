namespace CoreBusinessService.Models;

using System.ComponentModel.DataAnnotations.Schema;

public class AccessEvent
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DoorId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Result { get; set; } = "granted";
    public DateTime EventTime { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public string? CardId { get; set; }

    [NotMapped]
    public string? GateId { get; set; }

    [NotMapped]
    public string? Direction { get; set; }

    [NotMapped]
    public DateTime? Timestamp { get; set; }
}
