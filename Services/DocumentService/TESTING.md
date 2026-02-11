# DocumentService Testing Guide

## Prerequisites

1. PostgreSQL running with database `document_db`
2. Valid JWT token with appropriate role claims
3. Storage directory exists (auto-created on first upload)

## Configuration

### Storage Path (appsettings.json)

```json
{
  "Storage": {
    "RootPath": "C:\\erp_uploads\\documents"
  }
}
```

The directory will be auto-created on first upload if it doesn't exist.

## EF Migrations

```bash
# From the DocumentService folder
cd C:\Users\daniel.achigbue\source\repos\services\DocumentService

# Add initial migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

## Quick API Test Calls

Use Swagger UI at `https://localhost:{port}/swagger`.

### 1. Upload Document (Tenant)

```bash
curl -X POST https://localhost:7089/api/v1/documents/upload \
  -H "Authorization: Bearer {tenant_token}" \
  -F "file=@/path/to/document.pdf" \
  -F "documentFor=Tenant" \
  -F "entityId=3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c" \
  -F "type=IdCard" \
  -F "visibility=Private" \
  -F "notes=My ID card"
```

### 2. Upload Document (Staff for Property)

```bash
curl -X POST https://localhost:7089/api/v1/documents/upload \
  -H "Authorization: Bearer {staff_token}" \
  -F "file=@/path/to/lease.pdf" \
  -F "documentFor=Property" \
  -F "entityId=8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6" \
  -F "tenantUserId=3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c" \
  -F "type=LeaseAgreement" \
  -F "visibility=StaffOnly" \
  -F "notes=Lease agreement for unit 3B"
```

### 3. Get Document Metadata

```bash
curl -X GET https://localhost:7089/api/v1/documents/{documentId} \
  -H "Authorization: Bearer {token}"
```

### 4. Download Document

```bash
curl -X GET https://localhost:7089/api/v1/documents/{documentId}/download \
  -H "Authorization: Bearer {token}" \
  -o downloaded_file.pdf
```

### 5. List Documents for Entity

```bash
# Documents for a property
curl -X GET "https://localhost:7089/api/v1/documents/Property/8aaf1f93-0b2d-4f52-a0dc-e3e9b46355a6?take=50" \
  -H "Authorization: Bearer {token}"

# Documents for a tenant
curl -X GET "https://localhost:7089/api/v1/documents/Tenant/3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c?take=50" \
  -H "Authorization: Bearer {token}"
```

### 6. Update Document Metadata

```bash
curl -X PATCH https://localhost:7089/api/v1/documents/{documentId} \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "LeaseAgreement",
    "visibility": "StaffOnly",
    "notes": "Updated notes",
    "expiresAt": "2027-12-31T00:00:00Z"
  }'
```

### 7. Delete Document (Soft Delete)

```bash
curl -X DELETE https://localhost:7089/api/v1/documents/{documentId} \
  -H "Authorization: Bearer {token}"
```

### 8. Verify Idempotency (Duplicate Upload)

Upload the same file twice to the same entity - the second upload should return the existing document:

```bash
# First upload
curl -X POST https://localhost:7089/api/v1/documents/upload \
  -H "Authorization: Bearer {token}" \
  -F "file=@/path/to/document.pdf" \
  -F "documentFor=Tenant" \
  -F "entityId=3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c" \
  -F "type=IdCard" \
  -F "visibility=Private"

# Second upload (same file, same entity) - returns existing document
curl -X POST https://localhost:7089/api/v1/documents/upload \
  -H "Authorization: Bearer {token}" \
  -F "file=@/path/to/document.pdf" \
  -F "documentFor=Tenant" \
  -F "entityId=3842db61-7eeb-4daf-aaeb-bdf0f3a1f33c" \
  -F "type=IdCard" \
  -F "visibility=Private"
```

## Verification Checklist

### File Storage
- [ ] File exists on disk at `{RootPath}\{yyyy}\{MM}\{guid}_{filename}`
- [ ] SHA256 checksum is computed and stored
- [ ] File size matches uploaded file

### Database
- [ ] Row exists in `documents` table
- [ ] `storage_path` matches actual file location
- [ ] `checksum_sha256` is populated
- [ ] `created_at` is UTC

### Soft Delete
- [ ] `deleted_at` is set on delete
- [ ] Document no longer appears in queries
- [ ] File remains on disk (not physically deleted)

### Tenant Scoping
- [ ] Tenant can only upload for `Tenant` or `Booking`
- [ ] Tenant can only access documents where `TenantUserId` matches
- [ ] Staff can access all documents

## Authorization Policies

| Policy | Roles |
|--------|-------|
| `document.read` | super_admin, manager, support, sales, tenant |
| `document.write` | super_admin, manager, support, sales, tenant |
| `document.manage` | super_admin, manager, support |
| `document.internal.write` | super_admin, manager, internal |

## Access Rules

### Tenant
- Can only upload documents for `Tenant` or `Booking` entities
- `TenantUserId` is always set to caller's ID
- Can only view/download/delete own documents (where `TenantUserId` matches)
- Can only update `Notes` and `Visibility` (to `Private`)

### Staff (CanManage)
- Can upload for any entity type
- Can specify any `TenantUserId` or leave null
- Can access all documents
- Can update all fields

## Enums Reference

**DocumentFor**: Tenant, Property, Booking, Owner, SupportTicket, General

**DocumentType**: IdCard, Passport, LeaseAgreement, OwnershipProof, InspectionPhoto, MaintenanceReceipt, Other

**DocumentVisibility**: Private, StaffOnly, PublicLink

## File Storage Structure

```
C:\erp_uploads\documents\
├── 2026\
│   ├── 01\
│   │   ├── {guid}_document1.pdf
│   │   └── {guid}_document2.jpg
│   └── 02\
│       └── {guid}_lease_agreement.pdf
```

## Database Schema

```sql
CREATE TABLE documents (
    "Id" uuid PRIMARY KEY,
    "DocumentFor" integer NOT NULL,
    "EntityId" uuid NOT NULL,
    "TenantUserId" uuid,
    "UploadedByUserId" uuid NOT NULL,
    "Type" integer NOT NULL,
    "Visibility" integer NOT NULL,
    "FileName" varchar(255) NOT NULL,
    "ContentType" varchar(100) NOT NULL,
    "SizeBytes" bigint NOT NULL,
    "StoragePath" varchar(500) NOT NULL,
    "ChecksumSha256" varchar(64),
    "ExpiresAt" timestamp with time zone,
    "Notes" varchar(2000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "DeletedAt" timestamp with time zone
);

-- Indexes
CREATE INDEX ix_documents_entity_id ON documents ("EntityId");
CREATE INDEX ix_documents_tenant_user_id ON documents ("TenantUserId");
CREATE INDEX ix_documents_created_at ON documents ("CreatedAt");
CREATE INDEX ix_documents_document_for_entity_id ON documents ("DocumentFor", "EntityId");
CREATE UNIQUE INDEX ix_documents_checksum ON documents ("DocumentFor", "EntityId", "ChecksumSha256")
    WHERE "ChecksumSha256" IS NOT NULL;
```
