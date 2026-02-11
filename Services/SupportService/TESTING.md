# SupportService Testing Guide

## Prerequisites

1. PostgreSQL running with database `support_db`
2. Valid JWT token with appropriate role claims

## EF Migrations

```bash
# From the SupportService folder
cd C:\Users\daniel.achigbue\source\repos\services\SupportService

# Add initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

## Quick API Test Calls (10 scenarios)

Use Swagger UI at `https://localhost:{port}/swagger` or these curl commands.

### 1. Create Ticket

```bash
curl -X POST https://localhost:7087/api/v1/tickets \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "subject": "Cannot access my unit",
    "description": "I have been trying to access my unit but the door code does not work.",
    "category": "Property",
    "priority": "High",
    "propertyId": "8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6",
    "bookingId": null
  }'
```

### 2. Get Ticket by ID

```bash
curl -X GET https://localhost:7087/api/v1/tickets/{ticketId} \
  -H "Authorization: Bearer {token}"
```

### 3. Assign Ticket to Staff

```bash
curl -X PATCH https://localhost:7087/api/v1/tickets/{ticketId}/assign \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "assignedToUserId": "3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c"
  }'
```

### 4. Update Ticket Status

```bash
curl -X PATCH https://localhost:7087/api/v1/tickets/{ticketId}/status \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "InProgress",
    "notes": "Looking into this issue"
  }'
```

### 5. Add Message to Ticket

```bash
curl -X POST https://localhost:7087/api/v1/tickets/{ticketId}/messages \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "body": "We have reset your door code. Please try 1234."
  }'
```

### 6. Get Ticket Messages

```bash
curl -X GET https://localhost:7087/api/v1/tickets/{ticketId}/messages \
  -H "Authorization: Bearer {token}"
```

### 7. Get Ticket Activities (Audit Trail)

```bash
curl -X GET https://localhost:7087/api/v1/tickets/{ticketId}/activities \
  -H "Authorization: Bearer {token}"
```

### 8. Get All Tickets for a Tenant

```bash
curl -X GET https://localhost:7087/api/v1/tenants/{tenantUserId}/tickets \
  -H "Authorization: Bearer {token}"
```

### 9. Delete Ticket (Soft Delete)

```bash
curl -X DELETE https://localhost:7087/api/v1/tickets/{ticketId} \
  -H "Authorization: Bearer {token}"
```

### 10. Verify Soft Delete Filter

After deleting a ticket, verify it no longer appears in queries:

```bash
# This should return 404 (ticket is soft-deleted)
curl -X GET https://localhost:7087/api/v1/tickets/{deletedTicketId} \
  -H "Authorization: Bearer {token}"

# List tickets for tenant - deleted ticket should NOT appear
curl -X GET https://localhost:7087/api/v1/tenants/{tenantUserId}/tickets \
  -H "Authorization: Bearer {token}"
```

## Authorization Policies

| Policy | Roles |
|--------|-------|
| `support.read` | super_admin, manager, support, sales, tenant |
| `support.write` | super_admin, manager, support, sales, tenant |
| `support.manage` | super_admin, manager, support |
| `support.internal.write` | super_admin, manager |

## Access Rules

- **Tenant**: Can create tickets (TenantUserId auto-set from token)
- **Tenant**: Can only view/update/message own tickets
- **Tenant**: Can only change status to `Closed`
- **Staff (CanManage)**: Can access all tickets
- **Staff**: Can assign tickets and change to any status
- **Anonymous tickets** (TenantUserId = null): Staff-only access

## Enums Reference

**TicketStatus**: Open, InProgress, Resolved, Closed

**TicketPriority**: Low, Medium, High, Urgent

**TicketCategory**: General, Payment, Booking, Property, Maintenance, Account, Other

**TicketMessageType**: Customer, Staff, System

## Activity Events

The following events are logged to `ticket_activities`:

- `Created` - When ticket is created
- `Assigned` - When ticket is assigned to staff
- `StatusChanged` - When status changes
- `Commented` - When a message is added
- `Deleted` - When ticket is soft deleted
