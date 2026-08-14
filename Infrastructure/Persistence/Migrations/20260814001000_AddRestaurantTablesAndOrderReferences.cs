using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantTablesAndOrderReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TableId",
                table: "QrCodes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QrCodeId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TableId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_QrCodes_TenantId_Id",
                table: "QrCodes",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateTable(
                name: "RestaurantTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTables", x => x.Id);
                    table.UniqueConstraint("AK_RestaurantTables_TenantId_Id", x => new { x.TenantId, x.Id });
                    table.ForeignKey(
                        name: "FK_RestaurantTables_Branches_TenantId_BranchId",
                        columns: x => new { x.TenantId, x.BranchId },
                        principalTable: "Branches",
                        principalColumns: new[] { "TenantId", "Id" });
                });

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_TenantId_BranchId_TableId_TargetType",
                table: "QrCodes",
                columns: new[] { "TenantId", "BranchId", "TableId", "TargetType" });

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_TenantId_TableId",
                table: "QrCodes",
                columns: new[] { "TenantId", "TableId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_QrCodeId",
                table: "Orders",
                columns: new[] { "TenantId", "QrCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_TableId",
                table: "Orders",
                columns: new[] { "TenantId", "TableId" });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTables_TenantId_BranchId_Name",
                table: "RestaurantTables",
                columns: new[] { "TenantId", "BranchId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_QrCodes_TenantId_QrCodeId",
                table: "Orders",
                columns: new[] { "TenantId", "QrCodeId" },
                principalTable: "QrCodes",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_RestaurantTables_TenantId_TableId",
                table: "Orders",
                columns: new[] { "TenantId", "TableId" },
                principalTable: "RestaurantTables",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_QrCodes_RestaurantTables_TenantId_TableId",
                table: "QrCodes",
                columns: new[] { "TenantId", "TableId" },
                principalTable: "RestaurantTables",
                principalColumns: new[] { "TenantId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_QrCodes_TenantId_QrCodeId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_RestaurantTables_TenantId_TableId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_QrCodes_RestaurantTables_TenantId_TableId",
                table: "QrCodes");

            migrationBuilder.DropTable(
                name: "RestaurantTables");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_QrCodes_TenantId_Id",
                table: "QrCodes");

            migrationBuilder.DropIndex(
                name: "IX_QrCodes_TenantId_BranchId_TableId_TargetType",
                table: "QrCodes");

            migrationBuilder.DropIndex(
                name: "IX_QrCodes_TenantId_TableId",
                table: "QrCodes");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_QrCodeId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_TableId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "QrCodeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TableId",
                table: "Orders");
        }
    }
}
