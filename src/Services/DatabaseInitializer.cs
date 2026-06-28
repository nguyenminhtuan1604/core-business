using CoreBusinessService.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreBusinessService.Services;

public class DatabaseInitializer(
    AppDbContext db,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    private static readonly SemaphoreSlim Lock = new(1, 1);
    private static volatile bool ready;

    public async Task EnsureReadyAsync(CancellationToken cancellationToken)
    {
        if (ready)
        {
            return;
        }

        await Lock.WaitAsync(cancellationToken);
        try
        {
            if (ready)
            {
                return;
            }

            logger.LogInformation("Ensuring SQL Server database and tables exist.");
            await db.Database.EnsureCreatedAsync(cancellationToken);
            await EnsureCoreTablesAsync(cancellationToken);
            await EnsureIoTEventColumnsAsync(cancellationToken);
            ready = true;
            logger.LogInformation("SQL Server database schema is ready.");
        }
        finally
        {
            Lock.Release();
        }
    }

    private async Task EnsureCoreTablesAsync(CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[Alerts]', N'U') IS NULL
BEGIN
    CREATE TABLE [Alerts] (
        [Id] int NOT NULL IDENTITY,
        [Severity] nvarchar(20) NOT NULL,
        [Source] nvarchar(50) NOT NULL,
        [Message] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Alerts] PRIMARY KEY ([Id])
    );
END
""", cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[IoTEvents]', N'U') IS NULL
BEGIN
    CREATE TABLE [IoTEvents] (
        [Id] int NOT NULL IDENTITY,
        [DeviceId] nvarchar(100) NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [Temperature] decimal(5,2) NOT NULL,
        [Humidity] decimal(5,2) NULL,
        [Motion] bit NULL,
        [EventTime] datetime2 NOT NULL,
        CONSTRAINT [PK_IoTEvents] PRIMARY KEY ([Id])
    );
END
""", cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[AccessEvents]', N'U') IS NULL
BEGIN
    CREATE TABLE [AccessEvents] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(100) NOT NULL,
        [DoorId] nvarchar(100) NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [Result] nvarchar(50) NOT NULL,
        [EventTime] datetime2 NOT NULL,
        CONSTRAINT [PK_AccessEvents] PRIMARY KEY ([Id])
    );
END
""", cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[VisionEvents]', N'U') IS NULL
BEGIN
    CREATE TABLE [VisionEvents] (
        [Id] int NOT NULL IDENTITY,
        [CameraId] nvarchar(100) NOT NULL,
        [Location] nvarchar(200) NOT NULL,
        [RiskLevel] nvarchar(20) NOT NULL,
        [Description] nvarchar(500) NULL,
        [EventTime] datetime2 NOT NULL,
        CONSTRAINT [PK_VisionEvents] PRIMARY KEY ([Id])
    );
END
""", cancellationToken);
    }

    private async Task EnsureIoTEventColumnsAsync(CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[IoTEvents]', N'U') IS NOT NULL AND COL_LENGTH(N'[IoTEvents]', N'Humidity') IS NULL
BEGIN
    ALTER TABLE [IoTEvents] ADD [Humidity] decimal(5,2) NULL;
END
""", cancellationToken);

        await db.Database.ExecuteSqlRawAsync("""
IF OBJECT_ID(N'[IoTEvents]', N'U') IS NOT NULL AND COL_LENGTH(N'[IoTEvents]', N'Motion') IS NULL
BEGIN
    ALTER TABLE [IoTEvents] ADD [Motion] bit NULL;
END
""", cancellationToken);
    }
}
