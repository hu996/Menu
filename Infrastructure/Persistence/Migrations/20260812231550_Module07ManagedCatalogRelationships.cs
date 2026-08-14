using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Module07ManagedCatalogRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Modifiers",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Modifiers",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Modifiers",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ModifierOptions",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "ModifierOptions",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "ModifierOptions",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Ingredients",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Ingredients",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Allergens",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Allergens",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "Allergens",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.Sql("UPDATE Ingredients SET NameEn = Name WHERE NameEn IS NULL;");
            migrationBuilder.Sql("UPDATE Allergens SET NameEn = Name WHERE NameEn IS NULL;");
            migrationBuilder.Sql("UPDATE Modifiers SET NameEn = Name WHERE NameEn IS NULL;");
            migrationBuilder.Sql("UPDATE ModifierOptions SET NameEn = Name WHERE NameEn IS NULL;");
            migrationBuilder.Sql(@"
INSERT INTO Allergens (Id, Name, NameEn, NameAr, IsActive, CreatedAtUtc, UpdatedAtUtc, TenantId)
SELECT NEWID(), i.Name, i.Name, NULL, 1, i.CreatedAtUtc, NULL, i.TenantId
FROM Ingredients i
WHERE i.IsAllergen = 1
  AND i.IsActive = 1
  AND NOT EXISTS (SELECT 1 FROM Allergens a WHERE a.TenantId = i.TenantId AND a.Name = i.Name);
");
            migrationBuilder.Sql(@"
INSERT INTO MenuItemAllergens (MenuItemId, AllergenId, Id, CreatedAtUtc, UpdatedAtUtc, TenantId)
SELECT link.MenuItemId, a.Id, NEWID(), link.CreatedAtUtc, NULL, link.TenantId
FROM MenuItemIngredients link
INNER JOIN Ingredients i ON i.Id = link.IngredientId AND i.TenantId = link.TenantId
INNER JOIN Allergens a ON a.TenantId = i.TenantId AND a.Name = i.Name
WHERE i.IsAllergen = 1
  AND NOT EXISTS (SELECT 1 FROM MenuItemAllergens existing WHERE existing.MenuItemId = link.MenuItemId AND existing.AllergenId = a.Id);
");
            migrationBuilder.DropColumn(
                name: "IsAllergen",
                table: "Ingredients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Modifiers");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Modifiers");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "ModifierOptions");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "ModifierOptions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "Allergens");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "Allergens");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Modifiers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ModifierOptions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(160)",
                oldMaxLength: 160);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Ingredients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(160)",
                oldMaxLength: 160);

            migrationBuilder.AddColumn<bool>(
                name: "IsAllergen",
                table: "Ingredients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Allergens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(160)",
                oldMaxLength: 160);
        }
    }
}
