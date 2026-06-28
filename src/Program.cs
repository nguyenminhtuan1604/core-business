using CoreBusinessService.Data;
using CoreBusinessService.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAlertRuleService, AlertRuleService>();
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
builder.Services.AddHttpClient<INotificationService, NotificationService>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var timeoutSeconds = int.TryParse(configuration["REQUEST_TIMEOUT_SECONDS"], out var value)
        ? value
        : 5;

    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost,1433;Database=CoreBusinessDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));

builder.Services.AddSingleton<CoreBusinessService.Services.AnalyticsMqttService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "core-business-service",
    version = "1.0.0"
}));

app.MapControllers();

_ = Task.Run(() => EnsureDatabaseReadyAsync(app.Services, app.Logger));

app.Run();

static async Task EnsureDatabaseReadyAsync(IServiceProvider services, ILogger logger)
{
    const int maxAttempts = 10;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();

            logger.LogInformation(
                "Checking database readiness. Attempt {Attempt}/{MaxAttempts}",
                attempt,
                maxAttempts);

            await initializer.EnsureReadyAsync(CancellationToken.None);
            logger.LogInformation("Database is ready.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                ex,
                "Database is not ready yet. Retrying in 2 seconds. Attempt {Attempt}/{MaxAttempts}",
                attempt,
                maxAttempts);

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Database initialization failed after {MaxAttempts} attempts. The API will keep running; /health remains available.",
                maxAttempts);
        }
    }
}

