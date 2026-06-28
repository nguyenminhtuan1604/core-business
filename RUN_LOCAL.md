# Run Local

## Prerequisites

- Docker Desktop
- .NET 8 SDK, optional for non-Docker development

## Docker Compose Demo Run

From the `core-business-service` directory:

```powershell
docker compose down
docker compose up -d --build
docker compose ps
curl http://localhost:8090/health
```

Expected health response:

```json
{
  "status": "ok",
  "service": "core-business-service",
  "version": "1.0.0"
}
```

## Ports

- B6 API host port: `8090`
- B6 API container port: `8080`
- SQL Server host port: `1434`
- SQL Server Docker network port: `1433`

## B7 Notification Configuration

Default:

```text
NOTIFICATION_SERVICE_URL=http://localhost:8000
REQUEST_TIMEOUT_SECONDS=5
```

If B7 is running on the host and B6 is running inside Docker Desktop, use:

```powershell
$env:NOTIFICATION_SERVICE_URL="http://host.docker.internal:8000"
docker compose up -d --build
```

B6 does not crash if B7 is unavailable. It logs the failure and returns `notification_sent: false`.

## Run Without Docker For Development

Start SQL Server first, then run:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1434;Database=CoreBusinessDb;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;"
$env:ASPNETCORE_URLS="http://0.0.0.0:8080"
$env:NOTIFICATION_SERVICE_URL="http://localhost:8000"
$env:REQUEST_TIMEOUT_SECONDS="5"
dotnet run --project .\src\CoreBusinessService.csproj
```

The app creates database tables automatically on startup.
