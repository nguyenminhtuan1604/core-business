# Endpoint Catalog

Base URL for local demo:

```text
http://localhost:8090
```

## GET /health

Returns service status.

Response:

```json
{
  "status": "ok",
  "service": "core-business-service",
  "version": "1.0.0"
}
```

## POST /api/iot-events

Creates an IoT event. If `temperature > 35`, B6 creates a `high` severity alert and calls B7 Notification.

Request:

```json
{
  "device_id": "temp-sensor-a1",
  "location": "Building A - Level 2",
  "temperature": 36.5,
  "event_time": "2026-06-17T15:00:00Z"
}
```

Alert response includes:

```json
{
  "alert_created": true,
  "notification_sent": false,
  "notification_error": "B7 notification error: Connection refused"
}
```

## POST /api/access-events

Creates an access event. If `event_time` is outside `22:00-05:00`, B6 creates a `medium` severity alert and calls B7 Notification.

Request:

```json
{
  "user_id": "student-1001",
  "door_id": "door-lab-01",
  "location": "Lab 01",
  "result": "granted",
  "event_time": "2026-06-17T23:30:00Z"
}
```

## POST /api/vision-events

Creates a vision event. If `risk_level` is `high`, B6 creates a `high` severity alert and calls B7 Notification.

Request:

```json
{
  "camera_id": "cam-gate-01",
  "location": "Main Gate",
  "risk_level": "high",
  "description": "Crowd risk detected",
  "event_time": "2026-06-17T10:00:00Z"
}
```

## GET /api/alerts

Returns alerts ordered by newest first.

## POST /api/alerts

Creates an alert manually and calls B7 Notification.

Request:

```json
{
  "severity": "medium",
  "source": "manual",
  "message": "Manual operational alert"
}
```

## B6 -> B7 Notification Payload

```json
{
  "alert_id": "1",
  "severity": "high",
  "message": "High risk vision event detected at Main Gate",
  "source": "vision",
  "target": "security_team"
}
```
