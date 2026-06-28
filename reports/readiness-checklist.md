# Buoi 6 Readiness Checklist

Use this checklist for the B6 Core Business demo evidence.

## A. Nghiep Vu Ro Rang

- [ ] Explain B6 responsibility: receive events, apply rules, create alerts.
- [ ] Show rules:
  - `temperature > 35` creates `severity = high`.
  - `risk_level = high` creates `severity = high`.
  - access outside `22:00-05:00` creates `severity = medium`.

## B. Docker Compose Chay On Dinh

- [ ] Run:

```powershell
docker compose down
docker compose up -d --build
docker compose ps
```

- [ ] Capture screenshot in `evidence/screenshots/`.
- [ ] Save logs in `evidence/logs/`.

## C. Health Check Hoat Dong

- [ ] Local:

```powershell
curl http://localhost:8090/health
```

- [ ] Partner checks through 26.x.x.x IP:

```powershell
curl http://26.x.x.x:8090/health
```

Expected:

```json
{
  "status": "ok",
  "service": "core-business-service",
  "version": "1.0.0"
}
```

## D. Tich Hop Dung Contract

- [ ] B3/B4 sends IoT, access, or vision event into B6.
- [ ] B6 creates alert.
- [ ] B6 calls B7:

```http
POST {NOTIFICATION_SERVICE_URL}/api/notifications
```

## E. Payload Dung Schema

```json
{
  "alert_id": "1",
  "severity": "high",
  "message": "High risk vision event detected at Main Gate",
  "source": "vision",
  "target": "security_team"
}
```

## F. Xu Ly Loi Va Timeout

- [ ] Stop B7 or use invalid `NOTIFICATION_SERVICE_URL`.
- [ ] Send event that creates alert.
- [ ] Confirm B6 still returns success for alert creation:

```json
{
  "alert_created": true,
  "notification_sent": false,
  "notification_error": "..."
}
```

## G. Minh Chung Day Du

- [ ] `docker compose ps` screenshot.
- [ ] `/health` request-response screenshot.
- [ ] B3/B4 -> B6 event request-response screenshot.
- [ ] B6 -> B7 success or failure log.
- [ ] Save terminal/API logs in `evidence/logs/`.

## H. Trinh Bay Demo Ro Rang

- [ ] Introduce service boundary.
- [ ] Run health check.
- [ ] Send IoT/access/vision event.
- [ ] Show alert in `GET /api/alerts`.
- [ ] Show B7 notification behavior.
- [ ] Show timeout/failure handling when B7 is unavailable.
