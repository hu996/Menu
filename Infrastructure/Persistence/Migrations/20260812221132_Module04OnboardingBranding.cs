using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Module04OnboardingBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandAccentColor",
                table: "Tenants",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandPrimaryColor",
                table: "Tenants",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandAccentColorOverride",
                table: "Branches",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandPrimaryColorOverride",
                table: "Branches",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandAccentColor",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BrandPrimaryColor",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BrandAccentColorOverride",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "BrandPrimaryColorOverride",
                table: "Branches");
        }
    }
}
