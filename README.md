# core-business-service

Core Business Service for FIT4110 Smart Campus Operations Platform, group B6.

The service receives operational events from IoT, access control, and vision systems. It stores events in SQL Server, evaluates business rules, creates alerts, and forwards alert notifications to the B7 Notification service.

## Technology Stack

- ASP.NET Core Web API (.NET 8)
- SQL Server
- Entity Framework Core
- Docker
- Docker Compose

## Demo Endpoints

- `GET /health`
- `POST /api/iot-events`
- `POST /api/access-events`
- `POST /api/vision-events`
- `GET /api/alerts`
- `POST /api/alerts`

## Business Rules

- `temperature > 35` creates an alert with `severity = high`.
- `risk_level = high` creates an alert with `severity = high`.
- Access outside `22:00-05:00` creates an alert with `severity = medium`.

## B6 -> B7 Notification Contract

When B6 creates an alert, it sends:

```http
POST {NOTIFICATION_SERVICE_URL}/api/notifications
```

Payload:

```json
{
  "alert_id": "1",
  "severity": "high",
  "message": "High risk vision event detected at Main Gate",
  "source": "vision",
  "target": "security_team"
}
```

If B7 is down or times out, B6 keeps the alert and returns `notification_sent: false` with a clear `notification_error`.

## Run With Docker

```powershell
docker compose down
docker compose up -d --build
docker compose ps
curl http://localhost:8090/health
```

The API is available at:

```text
http://localhost:8090
```

SQL Server is published on host port `1434` and remains `sqlserver,1433` inside the Docker network.

Swagger UI:

```text
http://localhost:8090/swagger
```

## Environment

See `.env.example`.

```text
APP_PORT=8090
SQLSERVER_PORT=1434
DB_NAME=CoreBusinessDb
MSSQL_SA_PASSWORD=Your_strong_password123
NOTIFICATION_SERVICE_URL=http://localhost:8000
REQUEST_TIMEOUT_SECONDS=5
```

When B7 runs on the host machine and B6 runs in Docker, Docker Desktop may require:

```text
NOTIFICATION_SERVICE_URL=http://host.docker.internal:8000
```

## Expected Health Response

```json
{
  "status": "ok",
  "service": "core-business-service",
  "version": "1.0.0"
}
```
