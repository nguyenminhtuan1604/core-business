using CoreBusinessService.Models;
using Microsoft.EntityFrameworkCore;

namespace CoreBusinessService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<IoTEvent> IoTEvents => Set<IoTEvent>();
    public DbSet<AccessEvent> AccessEvents => Set<AccessEvent>();
    public DbSet<VisionEvent> VisionEvents => Set<VisionEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.Property(x => x.Severity).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<IoTEvent>(entity =>
        {
            entity.Property(x => x.DeviceId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Temperature).HasPrecision(5, 2);
            entity.Property(x => x.Humidity).HasPrecision(5, 2);
        });

        modelBuilder.Entity<AccessEvent>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DoorId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Result).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<VisionEvent>(entity =>
        {
            entity.Property(x => x.CameraId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RiskLevel).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
        });
    }
}
