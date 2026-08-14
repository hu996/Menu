using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchSpecificItemLocalizationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "BranchSpecificMenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "BranchSpecificMenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "BranchSpecificMenuItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "BranchSpecificMenuItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "BranchSpecificMenuItems");
        }
    }
}
