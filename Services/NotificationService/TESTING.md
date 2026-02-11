# NotificationService Testing Guide

## Prerequisites

1. PostgreSQL running with database `notification_db`
2. RabbitMQ running (for event consumption)
3. Valid JWT token with appropriate role claims

## RabbitMQ Docker Command

```bash
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

Management UI: http://localhost:15672 (guest/guest)

## EF Migrations

```bash
# From the NotificationService folder
cd C:\Users\daniel.achigbue\source\repos\services\NotificationService

# Add initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

## Quick API Test Calls

Use Swagger UI at `https://localhost:{port}/swagger` or these curl commands.

### 1. Create Notification via Internal Endpoint (Staff/Internal)

```bash
curl -X POST https://localhost:7088/api/v1/internal/notifications \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "recipientUserId": "3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c",
    "type": "Generic",
    "title": "Welcome to ERP",
    "message": "Your account has been created successfully.",
    "metadataJson": null,
    "channel": "InApp"
  }'
```

### 2. List Notifications (Own)

```bash
curl -X GET "https://localhost:7088/api/v1/notifications?take=50" \
  -H "Authorization: Bearer {token}"
```

### 3. List Notifications with Filter

```bash
# Only unread
curl -X GET "https://localhost:7088/api/v1/notifications?isRead=false&take=20" \
  -H "Authorization: Bearer {token}"

# Staff querying specific user
curl -X GET "https://localhost:7088/api/v1/notifications?userId=3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c" \
  -H "Authorization: Bearer {token}"
```

### 4. Get Notification by ID

```bash
curl -X GET https://localhost:7088/api/v1/notifications/{notificationId} \
  -H "Authorization: Bearer {token}"
```

### 5. Mark Notification as Read

```bash
curl -X PATCH https://localhost:7088/api/v1/notifications/{notificationId}/read \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "isRead": true
  }'
```

### 6. Mark Notification as Unread

```bash
curl -X PATCH https://localhost:7088/api/v1/notifications/{notificationId}/read \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "isRead": false
  }'
```

### 7. Delete Notification (Soft Delete)

```bash
curl -X DELETE https://localhost:7088/api/v1/notifications/{notificationId} \
  -H "Authorization: Bearer {token}"
```

### 8. Verify Soft Delete

After deleting, the notification should not appear in list:

```bash
# Should return 404
curl -X GET https://localhost:7088/api/v1/notifications/{deletedNotificationId} \
  -H "Authorization: Bearer {token}"
```

## Testing Event Consumers

### Option A: Via RabbitMQ Management UI

1. Go to http://localhost:15672
2. Navigate to Queues
3. Find the consumer queue (e.g., `NotificationService.Infrastructure.Consumers:BookingConfirmedConsumer`)
4. Publish a message with the event payload

### Option B: Trigger from Other Services

Confirm a booking in BookingService → triggers `BookingConfirmedEvent` → NotificationService creates notification.

### Sample Event Payloads

**BookingConfirmedEvent:**
```json
{
  "bookingId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "tenantUserId": "3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c",
  "propertyId": "8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6",
  "unitId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "startDate": "2026-03-01",
  "endDate": "2027-02-28"
}
```

**InvoiceCreatedEvent:**
```json
{
  "invoiceId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "tenantUserId": "3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c",
  "propertyId": "8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6",
  "amount": 150000.00,
  "dueDate": "2026-03-15",
  "type": "Rent"
}
```

**TicketCreatedEvent:**
```json
{
  "ticketId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
  "tenantUserId": "3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c",
  "createdByUserId": "3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c",
  "subject": "Door lock not working",
  "status": "Open"
}
```

## Authorization Policies

| Policy | Roles |
|--------|-------|
| `notification.read` | super_admin, manager, support, sales, tenant |
| `notification.write` | super_admin, manager, support, sales, tenant |
| `notification.manage` | super_admin, manager, support |
| `notification.internal.write` | super_admin, manager, internal |

## Access Rules

- **Tenant**: Can only read/update/delete own notifications
- **Staff (CanManage)**: Can access any user's notifications via `userId` query param
- **Internal endpoint**: Requires `notification.internal.write` policy

## Enums Reference

**NotificationChannel**: InApp, Email, Sms

**NotificationStatus**: Pending, Sent, Failed

**NotificationType**: BookingConfirmed, InvoiceCreated, RentDueReminder, TicketCreated, TicketUpdated, Generic

**RecipientType**: User, Owner

## MassTransit Consumers

| Consumer | Event | Creates Notification |
|----------|-------|---------------------|
| `BookingConfirmedConsumer` | BookingConfirmedEvent | Type=BookingConfirmed for TenantUserId |
| `InvoiceCreatedConsumer` | InvoiceCreatedEvent | Type=InvoiceCreated for TenantUserId |
| `TicketCreatedConsumer` | TicketCreatedEvent | Type=TicketCreated for CreatedByUserId |
