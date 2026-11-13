using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NashunumaApp.Infrastructure.Migrations;

public partial class AddUserLocationTransferAudit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "USER_LOCATION_TRANSFER_AUDIT",
            schema: "BISP_NASHUNUMA",
            columns: table => new
            {
                ID = table.Column<decimal>(type: "NUMBER", nullable: false)
                    .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                USER_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                USERNAME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                MODIFIED_BY = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                FROM_PROVINCE = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                FROM_PROVINCE_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                FROM_DISTRICT = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                FROM_DISTRICT_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                FROM_TEHSIL = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                FROM_TEHSIL_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                FROM_SITE_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                FROM_SITE_NAME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                TO_PROVINCE = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                TO_PROVINCE_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                TO_DISTRICT = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                TO_DISTRICT_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                TO_TEHSIL = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                TO_TEHSIL_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                TO_SITE_ID = table.Column<decimal>(type: "NUMBER", nullable: true),
                TO_SITE_NAME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: true),
                SYSTEM_NAME = table.Column<string>(type: "VARCHAR2(100)", maxLength: 100, nullable: false),
                TRANSFERRED_AT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                STATUS = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: false),
                FAILURE_REASON = table.Column<string>(type: "VARCHAR2(2000)", maxLength: 2000, nullable: true),
                TOTAL_TRANSFER_COUNT = table.Column<decimal>(type: "NUMBER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_USER_LOCATION_TRANSFER_AUDIT", x => x.ID);
            });

        migrationBuilder.CreateIndex(
            name: "IX_USER_LOCATION_TRANSFER_AUDIT_USERNAME_TRANSFERRED_AT",
            schema: "BISP_NASHUNUMA",
            table: "USER_LOCATION_TRANSFER_AUDIT",
            columns: new[] { "USERNAME", "TRANSFERRED_AT" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "USER_LOCATION_TRANSFER_AUDIT",
            schema: "BISP_NASHUNUMA");
    }
}
