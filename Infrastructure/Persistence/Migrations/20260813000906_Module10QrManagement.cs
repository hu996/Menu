using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Module10QrManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QrCodes_TenantId_BranchId",
                table: "QrCodes");

            migrationBuilder.AlterColumn<string>(
                name: "TargetType",
                table: "QrCodes",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "TableLabel",
                table: "QrCodes",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_TenantId_BranchId_TargetType_TableLabel",
                table: "QrCodes",
                columns: new[] { "TenantId", "BranchId", "TargetType", "TableLabel" },
                unique: true,
                filter: "[TableLabel] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QrCodes_TenantId_BranchId_TargetType_TableLabel",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "TableLabel",
                table: "QrCodes");

            migrationBuilder.AlterColumn<string>(
                name: "TargetType",
                table: "QrCodes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_TenantId_BranchId",
                table: "QrCodes",
                columns: new[] { "TenantId", "BranchId" });
        }
    }
}
