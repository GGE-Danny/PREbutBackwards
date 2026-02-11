using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AccountingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IncurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ledger_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpenseId = table.Column<Guid>(type: "uuid", nullable: true),
                    OwnerDisbursementId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "owner_disbursements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_owner_disbursements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "unit_rates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_rates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_Category",
                table: "expenses",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_IncurredAt",
                table: "expenses",
                column: "IncurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_PropertyId",
                table: "expenses",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_BookingId",
                table: "invoices",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_BookingId_Type",
                table: "invoices",
                columns: new[] { "BookingId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_DueDate",
                table: "invoices",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_PropertyId",
                table: "invoices",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_PropertyId_Type_Status",
                table: "invoices",
                columns: new[] { "PropertyId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_invoices_Status",
                table: "invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_TenantUserId",
                table: "invoices",
                column: "TenantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_TenantUserId_Status_DueDate",
                table: "invoices",
                columns: new[] { "TenantUserId", "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_CreatedAt",
                table: "ledger_entries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_EntryType",
                table: "ledger_entries",
                column: "EntryType");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_ExpenseId",
                table: "ledger_entries",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_InvoiceId",
                table: "ledger_entries",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_OwnerDisbursementId",
                table: "ledger_entries",
                column: "OwnerDisbursementId");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_entries_PaymentId",
                table: "ledger_entries",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_owner_disbursements_IsPaid",
                table: "owner_disbursements",
                column: "IsPaid");

            migrationBuilder.CreateIndex(
                name: "IX_owner_disbursements_OwnerId",
                table: "owner_disbursements",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_owner_disbursements_OwnerId_PropertyId_PeriodStart_PeriodEnd",
                table: "owner_disbursements",
                columns: new[] { "OwnerId", "PropertyId", "PeriodStart", "PeriodEnd" },
                unique: true,
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_owner_disbursements_PropertyId",
                table: "owner_disbursements",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_CreatedAt",
                table: "payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_payments_InvoiceId",
                table: "payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_PaymentMethod",
                table: "payments",
                column: "PaymentMethod");

            migrationBuilder.CreateIndex(
                name: "IX_payments_ReferenceId",
                table: "payments",
                column: "ReferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_unit_rates_IsActive",
                table: "unit_rates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_unit_rates_PropertyId",
                table: "unit_rates",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_unit_rates_UnitId",
                table: "unit_rates",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_unit_rates_UnitId_IsActive",
                table: "unit_rates",
                columns: new[] { "UnitId", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true AND \"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_unit_rates_UnitId_IsActive_EffectiveFrom_EffectiveTo",
                table: "unit_rates",
                columns: new[] { "UnitId", "IsActive", "EffectiveFrom", "EffectiveTo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "ledger_entries");

            migrationBuilder.DropTable(
                name: "owner_disbursements");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "unit_rates");

            migrationBuilder.DropTable(
                name: "invoices");
        }
    }
}
