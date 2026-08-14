using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Module03TenantIsolationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BranchMenuItemOverrides_Branches_BranchId",
                table: "BranchMenuItemOverrides");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchMenuItemOverrides_MenuItems_MenuItemId",
                table: "BranchMenuItemOverrides");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchMenus_Branches_BranchId",
                table: "BranchMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchMenus_Menus_MenuId",
                table: "BranchMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchSpecificMenuItems_Branches_BranchId",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchSpecificMenuItems_MenuCategories_CategoryId",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Branches_BranchId",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Menus_MenuId",
                table: "MenuCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemAllergens_Allergens_AllergenId",
                table: "MenuItemAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemAllergens_MenuItems_MenuItemId",
                table: "MenuItemAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemImages_MenuItems_MenuItemId",
                table: "MenuItemImages");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemIngredients_Ingredients_IngredientId",
                table: "MenuItemIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemIngredients_MenuItems_MenuItemId",
                table: "MenuItemIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemModifiers_MenuItems_MenuItemId",
                table: "MenuItemModifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemModifiers_Modifiers_ModifierId",
                table: "MenuItemModifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuCategories_MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ModifierOptions_Modifiers_ModifierId",
                table: "ModifierOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_Subscriptions_SubscriptionId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceHistories_Branches_BranchId",
                table: "PriceHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceHistories_MenuItems_MenuItemId",
                table: "PriceHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_QrCodes_Branches_BranchId",
                table: "QrCodes");

            migrationBuilder.DropIndex(
                name: "IX_QrCodes_BranchId",
                table: "QrCodes");

            migrationBuilder.DropIndex(
                name: "IX_PriceHistories_BranchId",
                table: "PriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_PriceHistories_MenuItemId",
                table: "PriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_SubscriptionId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ModifierOptions_ModifierId",
                table: "ModifierOptions");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemModifiers_ModifierId",
                table: "MenuItemModifiers");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemIngredients_IngredientId",
                table: "MenuItemIngredients");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemImages_MenuItemId",
                table: "MenuItemImages");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemAllergens_AllergenId",
                table: "MenuItemAllergens");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_MenuId",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_BranchId",
                table: "Memberships");

            migrationBuilder.DropIndex(
                name: "IX_BranchSpecificMenuItems_BranchId",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropIndex(
                name: "IX_BranchSpecificMenuItems_CategoryId",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropIndex(
                name: "IX_BranchMenus_MenuId",
                table: "BranchMenus");

            migrationBuilder.DropIndex(
                name: "IX_BranchMenuItemOverrides_BranchId",
                table: "BranchMenuItemOverrides");

            migrationBuilder.DropIndex(
                name: "IX_BranchMenuItemOverrides_MenuItemId",
                table: "BranchMenuItemOverrides");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Subscriptions_TenantId_Id",
                table: "Subscriptions",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Modifiers_TenantId_Id",
                table: "Modifiers",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Menus_TenantId_Id",
                table: "Menus",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MenuItems_TenantId_Id",
                table: "MenuItems",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MenuCategories_TenantId_Id",
                table: "MenuCategories",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Ingredients_TenantId_Id",
                table: "Ingredients",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Branches_TenantId_Id",
                table: "Branches",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Allergens_TenantId_Id",
                table: "Allergens",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_TenantId_BranchId",
                table: "QrCodes",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_TenantId_BranchId",
                table: "PriceHistories",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_TenantId_MenuItemId",
                table: "PriceHistories",
                columns: new[] { "TenantId", "MenuItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TenantId_SubscriptionId",
                table: "PaymentTransactions",
                columns: new[] { "TenantId", "SubscriptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_TenantId_MenuCategoryId",
                table: "MenuItems",
                columns: new[] { "TenantId", "MenuCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemModifiers_TenantId_MenuItemId",
                table: "MenuItemModifiers",
                columns: new[] { "TenantId", "MenuItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemIngredients_TenantId_IngredientId",
                table: "MenuItemIngredients",
                columns: new[] { "TenantId", "IngredientId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemIngredients_TenantId_MenuItemId",
                table: "MenuItemIngredients",
                columns: new[] { "TenantId", "MenuItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemImages_TenantId_MenuItemId",
                table: "MenuItemImages",
                columns: new[] { "TenantId", "MenuItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemAllergens_TenantId_AllergenId",
                table: "MenuItemAllergens",
                columns: new[] { "TenantId", "AllergenId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemAllergens_TenantId_MenuItemId",
                table: "MenuItemAllergens",
                columns: new[] { "TenantId", "MenuItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_TenantId_MenuId",
                table: "MenuCategories",
                columns: new[] { "TenantId", "MenuId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_TenantId_ParentCategoryId",
                table: "MenuCategories",
                columns: new[] { "TenantId", "ParentCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_TenantId_BranchId",
                table: "Memberships",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchSpecificMenuItems_TenantId_BranchId",
                table: "BranchSpecificMenuItems",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchSpecificMenuItems_TenantId_CategoryId",
                table: "BranchSpecificMenuItems",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchMenus_TenantId_BranchId",
                table: "BranchMenus",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchMenus_TenantId_MenuId",
                table: "BranchMenus",
                columns: new[] { "TenantId", "MenuId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchMenuItemOverrides_TenantId_MenuItemId",
                table: "BranchMenuItemOverrides",
                columns: new[] { "TenantId", "MenuItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_TenantId_MenuId",
                table: "AnalyticsEvents",
                columns: new[] { "TenantId", "MenuId" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_TenantId_MenuItemId",
                table: "AnalyticsEvents",
                columns: new[] { "TenantId", "MenuItemId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_Branches_TenantId_BranchId",
                table: "AnalyticsEvents",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_MenuItems_TenantId_MenuItemId",
                table: "AnalyticsEvents",
                columns: new[] { "TenantId", "MenuItemId" },
                principalTable: "MenuItems",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_Menus_TenantId_MenuId",
                table: "AnalyticsEvents",
                columns: new[] { "TenantId", "MenuId" },
                principalTable: "Menus",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMenuItemOverrides_Branches_TenantId_BranchId",
                table: "BranchMenuItemOverrides",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMenuItemOverrides_MenuItems_TenantId_MenuItemId",
                table: "BranchMenuItemOverrides",
                columns: new[] { "TenantId", "MenuItemId" },
                principalTable: "MenuItems",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMenus_Branches_TenantId_BranchId",
                table: "BranchMenus",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMenus_Menus_TenantId_MenuId",
                table: "BranchMenus",
                columns: new[] { "TenantId", "MenuId" },
                principalTable: "Menus",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_BranchSpecificMenuItems_Branches_TenantId_BranchId",
                table: "BranchSpecificMenuItems",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_BranchSpecificMenuItems_MenuCategories_TenantId_CategoryId",
                table: "BranchSpecificMenuItems",
                columns: new[] { "TenantId", "CategoryId" },
                principalTable: "MenuCategories",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Branches_TenantId_BranchId",
                table: "Memberships",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_MenuCategories_TenantId_ParentCategoryId",
                table: "MenuCategories",
                columns: new[] { "TenantId", "ParentCategoryId" },
                principalTable: "MenuCategories",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_Menus_TenantId_MenuId",
                table: "MenuCategories",
                columns: new[] { "TenantId", "MenuId" },
                principalTable: "Menus",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemAllergens_Allergens_TenantId_AllergenId",
                table: "MenuItemAllergens",
                columns: new[] { "TenantId", "AllergenId" },
                principalTable: "Allergens",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemAllergens_MenuItems_TenantId_MenuItemId",
                table: "MenuItemAllergens",
                columns: new[] { "TenantId", "MenuItemId" },
                principalTable: "MenuItems",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemImages_MenuItems_TenantId_MenuItemId",
                table: "MenuItemImages",
                columns: new[] { "TenantId", "MenuItemId" },
                principalTable: "MenuItems",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemIngredients_Ingredients_TenantId_IngredientId",
                table: "MenuItemIngredients",
                columns: new[] { "TenantId", "IngredientId" },
                principalTable: "Ingredients",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemIngredients_MenuItems_TenantId_MenuItemId",
                table: "MenuItemIngredients",
                columns: new[] { "TenantId", "MenuItemId" },
                principalTable: "MenuItems",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemModifiers_MenuItems_TenantId_MenuItemId",
                table: "MenuItemModifiers",
                columns: new[] { "TenantId", "MenuItemId" },
                principalTable: "MenuItems",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemModifiers_Modifiers_TenantId_ModifierId",
                table: "MenuItemModifiers",
                columns: new[] { "TenantId", "ModifierId" },
                principalTable: "Modifiers",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuCategories_TenantId_MenuCategoryId",
                table: "MenuItems",
                columns: new[] { "TenantId", "MenuCategoryId" },
                principalTable: "MenuCategories",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModifierOptions_Modifiers_TenantId_ModifierId",
                table: "ModifierOptions",
                columns: new[] { "TenantId", "ModifierId" },
                principalTable: "Modifiers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_Subscriptions_TenantId_SubscriptionId",
                table: "PaymentTransactions",
                columns: new[] { "TenantId", "SubscriptionId" },
                principalTable: "Subscriptions",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_PriceHistories_Branches_TenantId_BranchId",
                table: "PriceHistories",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_PriceHistories_MenuItems_TenantId_MenuItemId",
                table: "PriceHistories",
                columns: new[] { "TenantId", "MenuItemId" },
                principalTable: "MenuItems",
                principalColumns: new[] { "TenantId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_QrCodes_Branches_TenantId_BranchId",
                table: "QrCodes",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_Branches_TenantId_BranchId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_MenuItems_TenantId_MenuItemId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_Menus_TenantId_MenuId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchMenuItemOverrides_Branches_TenantId_BranchId",
                table: "BranchMenuItemOverrides");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchMenuItemOverrides_MenuItems_TenantId_MenuItemId",
                table: "BranchMenuItemOverrides");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchMenus_Branches_TenantId_BranchId",
                table: "BranchMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchMenus_Menus_TenantId_MenuId",
                table: "BranchMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchSpecificMenuItems_Branches_TenantId_BranchId",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchSpecificMenuItems_MenuCategories_TenantId_CategoryId",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Memberships_Branches_TenantId_BranchId",
                table: "Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_MenuCategories_TenantId_ParentCategoryId",
                table: "MenuCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuCategories_Menus_TenantId_MenuId",
                table: "MenuCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemAllergens_Allergens_TenantId_AllergenId",
                table: "MenuItemAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemAllergens_MenuItems_TenantId_MenuItemId",
                table: "MenuItemAllergens");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemImages_MenuItems_TenantId_MenuItemId",
                table: "MenuItemImages");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemIngredients_Ingredients_TenantId_IngredientId",
                table: "MenuItemIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemIngredients_MenuItems_TenantId_MenuItemId",
                table: "MenuItemIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemModifiers_MenuItems_TenantId_MenuItemId",
                table: "MenuItemModifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemModifiers_Modifiers_TenantId_ModifierId",
                table: "MenuItemModifiers");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuCategories_TenantId_MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ModifierOptions_Modifiers_TenantId_ModifierId",
                table: "ModifierOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_Subscriptions_TenantId_SubscriptionId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceHistories_Branches_TenantId_BranchId",
                table: "PriceHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceHistories_MenuItems_TenantId_MenuItemId",
                table: "PriceHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_QrCodes_Branches_TenantId_BranchId",
                table: "QrCodes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Subscriptions_TenantId_Id",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_QrCodes_TenantId_BranchId",
                table: "QrCodes");

            migrationBuilder.DropIndex(
                name: "IX_PriceHistories_TenantId_BranchId",
                table: "PriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_PriceHistories_TenantId_MenuItemId",
                table: "PriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_TenantId_SubscriptionId",
                table: "PaymentTransactions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Modifiers_TenantId_Id",
                table: "Modifiers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Menus_TenantId_Id",
                table: "Menus");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_MenuItems_TenantId_Id",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_TenantId_MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemModifiers_TenantId_MenuItemId",
                table: "MenuItemModifiers");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemIngredients_TenantId_IngredientId",
                table: "MenuItemIngredients");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemIngredients_TenantId_MenuItemId",
                table: "MenuItemIngredients");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemImages_TenantId_MenuItemId",
                table: "MenuItemImages");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemAllergens_TenantId_AllergenId",
                table: "MenuItemAllergens");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemAllergens_TenantId_MenuItemId",
                table: "MenuItemAllergens");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_MenuCategories_TenantId_Id",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_TenantId_MenuId",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_TenantId_ParentCategoryId",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_Memberships_TenantId_BranchId",
                table: "Memberships");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Ingredients_TenantId_Id",
                table: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_BranchSpecificMenuItems_TenantId_BranchId",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropIndex(
                name: "IX_BranchSpecificMenuItems_TenantId_CategoryId",
                table: "BranchSpecificMenuItems");

            migrationBuilder.DropIndex(
                name: "IX_BranchMenus_TenantId_BranchId",
                table: "BranchMenus");

            migrationBuilder.DropIndex(
                name: "IX_BranchMenus_TenantId_MenuId",
                table: "BranchMenus");

            migrationBuilder.DropIndex(
                name: "IX_BranchMenuItemOverrides_TenantId_MenuItemId",
                table: "BranchMenuItemOverrides");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Branches_TenantId_Id",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_TenantId_MenuId",
                table: "AnalyticsEvents");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_TenantId_MenuItemId",
                table: "AnalyticsEvents");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Allergens_TenantId_Id",
                table: "Allergens");

            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_BranchId",
                table: "QrCodes",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_BranchId",
                table: "PriceHistories",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_MenuItemId",
                table: "PriceHistories",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_SubscriptionId",
                table: "PaymentTransactions",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ModifierOptions_ModifierId",
                table: "ModifierOptions",
                column: "ModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MenuCategoryId",
                table: "MenuItems",
                column: "MenuCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemModifiers_ModifierId",
                table: "MenuItemModifiers",
                column: "ModifierId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemIngredients_IngredientId",
                table: "MenuItemIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemImages_MenuItemId",
                table: "MenuItemImages",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemAllergens_AllergenId",
                table: "MenuItemAllergens",
                column: "AllergenId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_MenuId",
                table: "MenuCategories",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_Memberships_BranchId",
                table: "Memberships",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchSpecificMenuItems_BranchId",
                table: "BranchSpecificMenuItems",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchSpecificMenuItems_CategoryId",
                table: "BranchSpecificMenuItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchMenus_MenuId",
                table: "BranchMenus",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchMenuItemOverrides_BranchId",
                table: "BranchMenuItemOverrides",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchMenuItemOverrides_MenuItemId",
                table: "BranchMenuItemOverrides",
                column: "MenuItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMenuItemOverrides_Branches_BranchId",
                table: "BranchMenuItemOverrides",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMenuItemOverrides_MenuItems_MenuItemId",
                table: "BranchMenuItemOverrides",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMenus_Branches_BranchId",
                table: "BranchMenus",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchMenus_Menus_MenuId",
                table: "BranchMenus",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchSpecificMenuItems_Branches_BranchId",
                table: "BranchSpecificMenuItems",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchSpecificMenuItems_MenuCategories_CategoryId",
                table: "BranchSpecificMenuItems",
                column: "CategoryId",
                principalTable: "MenuCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Memberships_Branches_BranchId",
                table: "Memberships",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuCategories_Menus_MenuId",
                table: "MenuCategories",
                column: "MenuId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemAllergens_Allergens_AllergenId",
                table: "MenuItemAllergens",
                column: "AllergenId",
                principalTable: "Allergens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemAllergens_MenuItems_MenuItemId",
                table: "MenuItemAllergens",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemImages_MenuItems_MenuItemId",
                table: "MenuItemImages",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemIngredients_Ingredients_IngredientId",
                table: "MenuItemIngredients",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemIngredients_MenuItems_MenuItemId",
                table: "MenuItemIngredients",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemModifiers_MenuItems_MenuItemId",
                table: "MenuItemModifiers",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemModifiers_Modifiers_ModifierId",
                table: "MenuItemModifiers",
                column: "ModifierId",
                principalTable: "Modifiers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuCategories_MenuCategoryId",
                table: "MenuItems",
                column: "MenuCategoryId",
                principalTable: "MenuCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModifierOptions_Modifiers_ModifierId",
                table: "ModifierOptions",
                column: "ModifierId",
                principalTable: "Modifiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_Subscriptions_SubscriptionId",
                table: "PaymentTransactions",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceHistories_Branches_BranchId",
                table: "PriceHistories",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceHistories_MenuItems_MenuItemId",
                table: "PriceHistories",
                column: "MenuItemId",
                principalTable: "MenuItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QrCodes_Branches_BranchId",
                table: "QrCodes",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
