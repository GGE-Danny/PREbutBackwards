using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnalyticsService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "booking_metrics_daily",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalBookings = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedBookings = table.Column<int>(type: "integer", nullable: false),
                    CancelledBookings = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_metrics_daily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "processed_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "revenue_metrics_monthly",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    RentCollected = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Expenses = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetRevenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revenue_metrics_monthly", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vacancy_metrics_monthly",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    OccupiedDays = table.Column<int>(type: "integer", nullable: false),
                    AvailableDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vacancy_metrics_monthly", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_booking_metrics_daily_Date",
                table: "booking_metrics_daily",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_booking_metrics_daily_PropertyId",
                table: "booking_metrics_daily",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_metrics_daily_PropertyId_UnitId_Date",
                table: "booking_metrics_daily",
                columns: new[] { "PropertyId", "UnitId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_MessageId",
                table: "processed_events",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_revenue_metrics_monthly_PropertyId",
                table: "revenue_metrics_monthly",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_revenue_metrics_monthly_PropertyId_Year_Month",
                table: "revenue_metrics_monthly",
                columns: new[] { "PropertyId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_revenue_metrics_monthly_Year_Month",
                table: "revenue_metrics_monthly",
                columns: new[] { "Year", "Month" });

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_metrics_monthly_PropertyId",
                table: "vacancy_metrics_monthly",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_metrics_monthly_PropertyId_UnitId_Year_Month",
                table: "vacancy_metrics_monthly",
                columns: new[] { "PropertyId", "UnitId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vacancy_metrics_monthly_Year_Month",
                table: "vacancy_metrics_monthly",
                columns: new[] { "Year", "Month" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_metrics_daily");

            migrationBuilder.DropTable(
                name: "processed_events");

            migrationBuilder.DropTable(
                name: "revenue_metrics_monthly");

            migrationBuilder.DropTable(
                name: "vacancy_metrics_monthly");
        }
    }
}
