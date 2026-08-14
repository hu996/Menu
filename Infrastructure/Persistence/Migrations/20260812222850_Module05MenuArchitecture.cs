using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Module05MenuArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "BrandAccentColor", table: "Menus", type: "nvarchar(16)", maxLength: 16, nullable: true);
            migrationBuilder.AddColumn<string>(name: "BrandPrimaryColor", table: "Menus", type: "nvarchar(16)", maxLength: 16, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Description", table: "Menus", type: "nvarchar(500)", maxLength: 500, nullable: true);
            migrationBuilder.AddColumn<string>(name: "DescriptionAr", table: "Menus", type: "nvarchar(500)", maxLength: 500, nullable: true);
            migrationBuilder.AddColumn<int>(name: "SortOrder", table: "Menus", type: "int", nullable: false, defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_TenantId_SortOrder",
                table: "Menus",
                columns: new[] { "TenantId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Menus_TenantId_SortOrder", table: "Menus");
            migrationBuilder.DropColumn(name: "BrandAccentColor", table: "Menus");
            migrationBuilder.DropColumn(name: "BrandPrimaryColor", table: "Menus");
            migrationBuilder.DropColumn(name: "Description", table: "Menus");
            migrationBuilder.DropColumn(name: "DescriptionAr", table: "Menus");
            migrationBuilder.DropColumn(name: "SortOrder", table: "Menus");
        }
    }
}
