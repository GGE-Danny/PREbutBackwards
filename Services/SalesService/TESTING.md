# SalesService Testing Guide

## Prerequisites

1. PostgreSQL running with database `sales_db`
2. RabbitMQ running (optional, for event consumption)
3. Valid JWT token with appropriate role claims

## EF Migrations

```bash
# From the SalesService folder
cd C:\Users\daniel.achigbue\source\repos\services\SalesService

# Add initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

## Quick API Test Calls (10 scenarios)

Use Swagger UI at `https://localhost:{port}/swagger` or these curl commands.

### 1. Create Lead

```bash
curl -X POST https://localhost:7086/api/v1/leads \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "phoneNumber": "+234801234567",
    "email": "john@example.com",
    "source": "WhatsApp",
    "propertyId": "8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6",
    "unitId": null,
    "notes": "Interested in 2-bedroom"
  }'
```

### 2. Assign Lead to Sales Agent

```bash
curl -X PATCH https://localhost:7086/api/v1/leads/{leadId}/assign \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "assignedToUserId": "3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c"
  }'
```

### 3. Update Lead Status

```bash
curl -X PATCH https://localhost:7086/api/v1/leads/{leadId}/status \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Contacted",
    "notes": "Called and discussed options"
  }'
```

### 4. Schedule Visit

```bash
curl -X POST https://localhost:7086/api/v1/visits \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "leadId": "{leadId}",
    "propertyId": "8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6",
    "unitId": null,
    "scheduledAtUtc": "2026-02-10T14:00:00Z",
    "notes": "Property tour scheduled"
  }'
```

### 5. Set Visit Outcome

```bash
curl -X PATCH https://localhost:7086/api/v1/visits/{visitId}/outcome \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "outcome": "Interested",
    "notes": "Client liked unit 3B, wants to proceed"
  }'
```

### 6. Create Commission Record

```bash
curl -X POST https://localhost:7086/api/v1/commissions \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "ownerId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "propertyId": "8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6",
    "unitId": null,
    "bookingId": null,
    "leadId": "{leadId}",
    "amount": 50000.00,
    "commissionPercent": 10.00,
    "earnedAtUtc": "2026-02-07T12:00:00Z",
    "notes": "Commission for lease agreement"
  }'
```

### 7. Update Commission Status

```bash
curl -X PATCH https://localhost:7086/api/v1/commissions/{commissionId}/status \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Earned",
    "notes": "Confirmed by finance"
  }'
```

### 8. Query Commissions

```bash
# By property
curl -X GET "https://localhost:7086/api/v1/commissions?propertyId=8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6" \
  -H "Authorization: Bearer {token}"

# By owner and date range
curl -X GET "https://localhost:7086/api/v1/commissions?ownerId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa&from=2026-01-01&to=2026-12-31" \
  -H "Authorization: Bearer {token}"
```

### 9. Delete Lead (Soft Delete)

```bash
curl -X DELETE https://localhost:7086/api/v1/leads/{leadId} \
  -H "Authorization: Bearer {token}"
```

### 10. Verify Soft Delete Filter

After deleting a lead, verify it no longer appears in queries:

```bash
# This should return 404 (lead is soft-deleted)
curl -X GET https://localhost:7086/api/v1/leads/{deletedLeadId} \
  -H "Authorization: Bearer {token}"

# List leads for property - deleted lead should NOT appear
curl -X GET https://localhost:7086/api/v1/properties/{propertyId}/leads \
  -H "Authorization: Bearer {token}"
```

## Authorization Policies

| Policy | Roles |
|--------|-------|
| `sales.read` | super_admin, manager, support, sales, tenant |
| `sales.write` | super_admin, manager, support, sales, tenant |
| `sales.manage` | super_admin, manager, sales |
| `sales.internal.write` | super_admin, manager |

## Tenant Access Rules

- Tenants can create leads for themselves (TenantUserId auto-set)
- Tenants can only view leads where TenantUserId matches their ID
- Staff (CanManage roles) can access all leads
- Anonymous leads (TenantUserId = null) are staff-only

## MassTransit Consumer

`BookingConfirmedConsumer` listens for `BookingConfirmedEvent`:
- Finds leads matching TenantUserId + PropertyId + UnitId
- Auto-converts matching leads to status `Converted`
- Best-effort: no error if no matching lead found

## Enums Reference

**LeadStatus**: New, Contacted, ViewingScheduled, ViewingDone, Converted, Lost

**VisitOutcome**: Pending, NoShow, Interested, NotInterested, Negotiating

**CommissionStatus**: Pending, Earned, Disputed, Settled
