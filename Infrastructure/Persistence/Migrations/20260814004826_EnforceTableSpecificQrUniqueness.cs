using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceTableSpecificQrUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QrCodes_TenantId_BranchId_TableId_TargetType",
                table: "QrCodes");

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_TenantId_BranchId_TableId_TargetType",
                table: "QrCodes",
                columns: new[] { "TenantId", "BranchId", "TableId", "TargetType" },
                unique: true,
                filter: "[TableId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QrCodes_TenantId_BranchId_TableId_TargetType",
                table: "QrCodes");

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_TenantId_BranchId_TableId_TargetType",
                table: "QrCodes",
                columns: new[] { "TenantId", "BranchId", "TableId", "TargetType" });
        }
    }
}
