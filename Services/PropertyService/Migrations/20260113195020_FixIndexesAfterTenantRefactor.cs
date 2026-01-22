using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyService.Migrations
{
    /// <inheritdoc />
    public partial class FixIndexesAfterTenantRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_PropertyId",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "TenantProfileId",
                table: "UnitOccupancies");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Properties");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantUserId",
                table: "UnitOccupancies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(@"
ALTER TABLE ""PropertyTimelineEvents""
ALTER COLUMN ""ActorUserId"" TYPE uuid
USING (
  CASE
    WHEN ""ActorUserId"" IS NULL OR ""ActorUserId"" = '' THEN NULL
    WHEN ""ActorUserId"" ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
      THEN ""ActorUserId""::uuid
    ELSE NULL
  END
);
");


            migrationBuilder.Sql(@"
ALTER TABLE ""PropertyMedia""
ALTER COLUMN ""UploadedByUserId"" TYPE uuid
USING (
  CASE
    WHEN ""UploadedByUserId"" IS NULL OR ""UploadedByUserId"" = '' THEN NULL
    WHEN ""UploadedByUserId"" ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
      THEN ""UploadedByUserId""::uuid
    ELSE NULL
  END
);
");


            migrationBuilder.Sql(@"
ALTER TABLE ""Properties""
ALTER COLUMN ""OwnerId"" TYPE uuid
USING (
  CASE
    WHEN ""OwnerId"" IS NULL OR ""OwnerId"" = '' THEN NULL
    WHEN ""OwnerId"" ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
      THEN ""OwnerId""::uuid
    ELSE NULL
  END
);
");


            migrationBuilder.Sql(@"
ALTER TABLE ""ConditionLogs""
ALTER COLUMN ""CreatedByUserId"" TYPE uuid
USING (
  CASE
    WHEN ""CreatedByUserId"" IS NULL OR ""CreatedByUserId"" = '' THEN NULL
    WHEN ""CreatedByUserId"" ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
      THEN ""CreatedByUserId""::uuid
    ELSE NULL
  END
);
");


            migrationBuilder.CreateIndex(
                name: "IX_Units_PropertyId_UnitNumber",
                table: "Units",
                columns: new[] { "PropertyId", "UnitNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_PropertyCode",
                table: "Properties",
                column: "PropertyCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_PropertyId_UnitNumber",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Properties_PropertyCode",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TenantUserId",
                table: "UnitOccupancies");

            migrationBuilder.AddColumn<string>(
                name: "TenantProfileId",
                table: "UnitOccupancies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ActorUserId",
                table: "PropertyTimelineEvents",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UploadedByUserId",
                table: "PropertyMedia",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Properties",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Properties",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "ConditionLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_PropertyId",
                table: "Units",
                column: "PropertyId");
        }
    }
}
