# Service Boundary

## Service Name

`core-business-service`

## Team

B6 Core Business.

## Responsibility

B6 owns core operational business rules for the Smart Campus Operations Platform. It receives event data, stores operational records, creates alerts, and forwards notification requests to B7.

## In Scope

- Receive IoT, access, and vision events.
- Store `IoTEvents`, `AccessEvents`, `VisionEvents`, and `Alerts`.
- Apply alert rules for temperature, risk level, and outside-hours access.
- Expose health and alert endpoints for demo and integration.
- Call B7 Notification service when an alert is created.
- Handle B7 timeout or downtime without crashing.

## Out of Scope

- Authentication and authorization.
- Device registry management.
- User identity lifecycle.
- Notification delivery execution, owned by B7.
- Long-term analytics dashboards.
- Machine learning model inference.

## Owned Data

- `Alerts`
- `IoTEvents`
- `AccessEvents`
- `VisionEvents`

## Upstream Systems

- B3/B4 event producers.
- IoT sensor gateway.
- Access control gateway.
- Vision analytics service.

## Downstream Integration

- B7 Notification service.
- Endpoint: `POST {NOTIFICATION_SERVICE_URL}/api/notifications`
- Timeout: `REQUEST_TIMEOUT_SECONDS`, default `5`.
- Failure behavior: log the error, keep B6 response successful for alert creation, and return `notification_sent: false`.
