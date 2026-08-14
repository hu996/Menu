using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeIngredientActiveDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE [Ingredients] SET [IsActive] = 1 WHERE [IsActive] = 0");
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Ingredients",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Ingredients",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }
    }
}
