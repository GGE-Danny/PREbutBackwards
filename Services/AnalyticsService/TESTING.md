# AnalyticsService Testing Guide

## Prerequisites

1. **PostgreSQL** running on `localhost:5432`
2. **RabbitMQ** running on `localhost:5672`
3. Database created: `erp_analytics`

```bash
# Create database (psql)
psql -U postgres -c "CREATE DATABASE erp_analytics;"
```

## Run Migrations

```bash
cd AnalyticsService
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Run the Service

```bash
dotnet run
```

Service runs on `https://localhost:5001` (or check console output for port).

## Get a Test JWT Token

Use the same JWT settings as other ERP services. Example token payload:

```json
{
  "sub": "user-id-here",
  "role": "Admin",
  "iss": "ERP2.0",
  "aud": "ERP2.0",
  "exp": 1893456000
}
```

Set the token in your requests:
```bash
TOKEN="your-jwt-token-here"
```

---

## API Endpoints

### 1. Get Daily Booking Metrics

```bash
curl -X GET "https://localhost:5001/api/v1/analytics/bookings/daily?propertyId=PROPERTY_GUID&from=2025-01-01&to=2025-01-31" \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

**Response:**
```json
[
  {
    "id": "guid",
    "propertyId": "guid",
    "unitId": "guid",
    "date": "2025-01-15",
    "totalBookings": 5,
    "confirmedBookings": 4,
    "cancelledBookings": 1
  }
]
```

### 2. Get Monthly Vacancy Metrics

```bash
curl -X GET "https://localhost:5001/api/v1/analytics/vacancy/monthly?propertyId=PROPERTY_GUID&year=2025" \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

**Response:**
```json
[
  {
    "id": "guid",
    "propertyId": "guid",
    "unitId": "guid",
    "year": 2025,
    "month": 1,
    "occupiedDays": 25,
    "availableDays": 30,
    "occupancyRate": 83.33
  }
]
```

### 3. Get Monthly Revenue Metrics

```bash
curl -X GET "https://localhost:5001/api/v1/analytics/revenue/monthly?propertyId=PROPERTY_GUID&year=2025" \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

**Response:**
```json
[
  {
    "id": "guid",
    "propertyId": "guid",
    "year": 2025,
    "month": 1,
    "rentCollected": 50000.00,
    "expenses": 12000.00,
    "netRevenue": 38000.00
  }
]
```

### 4. Get Top Properties by Bookings

```bash
curl -X GET "https://localhost:5001/api/v1/analytics/top/properties?from=2025-01-01&to=2025-12-31&take=10" \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

**Response:**
```json
[
  {
    "propertyId": "guid",
    "totalBookings": 150,
    "rank": 1
  },
  {
    "propertyId": "guid",
    "totalBookings": 120,
    "rank": 2
  }
]
```

---

## Internal Endpoints (Admin/System Only)

### 5. Record Payment (Internal)

```bash
curl -X POST "https://localhost:5001/api/v1/internal/analytics/payments" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "invoiceId": "11111111-1111-1111-1111-111111111111",
    "propertyId": "22222222-2222-2222-2222-222222222222",
    "tenantUserId": "33333333-3333-3333-3333-333333333333",
    "amount": 15000.00,
    "paidAtUtc": "2025-01-15T10:00:00Z",
    "type": "Rent"
  }' \
  -k
```

**Response:**
```json
{ "message": "Payment recorded" }
```

### 6. Record Expense (Internal)

```bash
curl -X POST "https://localhost:5001/api/v1/internal/analytics/expenses" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "expenseId": "44444444-4444-4444-4444-444444444444",
    "propertyId": "22222222-2222-2222-2222-222222222222",
    "amount": 5000.00,
    "incurredAtUtc": "2025-01-20T10:00:00Z",
    "category": "Maintenance"
  }' \
  -k
```

**Response:**
```json
{ "message": "Expense recorded" }
```

---

## Testing MassTransit Consumers

The service listens on these RabbitMQ queues:
- `analytics-booking-confirmed`
- `analytics-booking-cancelled`
- `analytics-payment-recorded`
- `analytics-expense-logged`

### Publish Test Events via RabbitMQ Management UI

1. Go to `http://localhost:15672` (guest/guest)
2. Navigate to **Exchanges** > `analytics-booking-confirmed`
3. Publish a message with routing key and JSON body:

**BookingConfirmedEvent:**
```json
{
  "bookingId": "55555555-5555-5555-5555-555555555555",
  "propertyId": "22222222-2222-2222-2222-222222222222",
  "unitId": "66666666-6666-6666-6666-666666666666",
  "tenantUserId": "33333333-3333-3333-3333-333333333333",
  "startDate": "2025-02-01",
  "endDate": "2025-02-28",
  "confirmedAt": "2025-01-25T14:00:00Z"
}
```

**BookingCancelledEvent:**
```json
{
  "bookingId": "55555555-5555-5555-5555-555555555555",
  "propertyId": "22222222-2222-2222-2222-222222222222",
  "unitId": "66666666-6666-6666-6666-666666666666",
  "cancelledAt": "2025-01-26T10:00:00Z"
}
```

**PaymentRecordedEvent:**
```json
{
  "invoiceId": "77777777-7777-7777-7777-777777777777",
  "propertyId": "22222222-2222-2222-2222-222222222222",
  "tenantUserId": "33333333-3333-3333-3333-333333333333",
  "amount": 20000.00,
  "paidAt": "2025-01-28T09:00:00Z",
  "type": "Rent"
}
```

**ExpenseLoggedEvent:**
```json
{
  "expenseId": "88888888-8888-8888-8888-888888888888",
  "propertyId": "22222222-2222-2222-2222-222222222222",
  "amount": 3500.00,
  "incurredAt": "2025-01-29T11:00:00Z",
  "category": "Utilities"
}
```

---

## Verify Data in PostgreSQL

```sql
-- Check booking metrics
SELECT * FROM "BookingMetricsDaily" ORDER BY "Date" DESC;

-- Check vacancy metrics
SELECT * FROM "VacancyMetricsMonthly" ORDER BY "Year" DESC, "Month" DESC;

-- Check revenue metrics
SELECT * FROM "RevenueMetricsMonthly" ORDER BY "Year" DESC, "Month" DESC;

-- Check processed events (idempotency)
SELECT * FROM "ProcessedEvents" ORDER BY "ProcessedAt" DESC;
```

---

## Idempotency Test

Send the same payment/expense/event twice. The second request should return:
```json
{ "message": "Already processed" }
```

And no duplicate records should be created in the database.
