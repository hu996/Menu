using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductionHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM [Users]
                    WHERE DATALENGTH([Email]) > 640
                       OR DATALENGTH([NormalizedEmail]) > 640
                       OR DATALENGTH([DisplayName]) > 240
                       OR DATALENGTH([PasswordHash]) > 1024
                       OR DATALENGTH([SecurityStamp]) > 128)
                    THROW 51000, 'Production hardening cannot shorten one or more Users values. Clean the oversized data and retry the migration.', 1;

                IF EXISTS (
                    SELECT 1
                    FROM [PaymentTransactions]
                    WHERE DATALENGTH([Provider]) > 128
                       OR DATALENGTH([ProviderReference]) > 400
                       OR DATALENGTH([Status]) > 64)
                    THROW 51001, 'Production hardening cannot shorten one or more PaymentTransactions values. Clean the oversized data and retry the migration.', 1;

                IF EXISTS (SELECT 1 FROM [Orders] WHERE DATALENGTH([Status]) > 64)
                    THROW 51002, 'Production hardening cannot shorten one or more Orders status values. Clean the oversized data and retry the migration.', 1;
                """);

            migrationBuilder.CreateTable(
                name: "DistributedCache",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(449)", maxLength: 449, nullable: false),
                    Value = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ExpiresAtTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SlidingExpirationInSeconds = table.Column<long>(type: "bigint", nullable: true),
                    AbsoluteExpiration = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedCache", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistributedCache_ExpiresAtTime",
                table: "DistributedCache",
                column: "ExpiresAtTime");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_BranchId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_TenantId_MenuCategoryId",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_TenantId_MenuId",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_BranchMenus_TenantId_MenuId",
                table: "BranchMenus");

            migrationBuilder.AlterColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentTransactions",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderReference",
                table: "PaymentTransactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "PaymentTransactions",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "PaymentTransactions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PaymentTransactions",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Orders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_TenantId_Status_CreatedAtUtc",
                table: "PaymentTransactions",
                columns: new[] { "TenantId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId_UsedAtUtc_ExpiresAtUtc",
                table: "PasswordResetTokens",
                columns: new[] { "UserId", "UsedAtUtc", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_BranchId_Status_CreatedAtUtc",
                table: "Orders",
                columns: new[] { "TenantId", "BranchId", "Status", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_TenantId_MenuCategoryId_IsAvailable_SortOrder",
                table: "MenuItems",
                columns: new[] { "TenantId", "MenuCategoryId", "IsAvailable", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_TenantId_MenuId_IsActive_SortOrder",
                table: "MenuCategories",
                columns: new[] { "TenantId", "MenuId", "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchMenus_TenantId_MenuId_IsActive",
                table: "BranchMenus",
                columns: new[] { "TenantId", "MenuId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributedCache");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_TenantId_Status_CreatedAtUtc",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_UserId_UsedAtUtc_ExpiresAtUtc",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TenantId_BranchId_Status_CreatedAtUtc",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_TenantId_MenuCategoryId_IsAvailable_SortOrder",
                table: "MenuItems");

            migrationBuilder.DropIndex(
                name: "IX_MenuCategories_TenantId_MenuId_IsActive_SortOrder",
                table: "MenuCategories");

            migrationBuilder.DropIndex(
                name: "IX_BranchMenus_TenantId_MenuId_IsActive",
                table: "BranchMenus");

            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "NormalizedEmail",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(320)",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(320)",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderReference",
                table: "PaymentTransactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "PaymentTransactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TenantId_BranchId",
                table: "Orders",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_TenantId_MenuCategoryId",
                table: "MenuItems",
                columns: new[] { "TenantId", "MenuCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_MenuCategories_TenantId_MenuId",
                table: "MenuCategories",
                columns: new[] { "TenantId", "MenuId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchMenus_TenantId_MenuId",
                table: "BranchMenus",
                columns: new[] { "TenantId", "MenuId" });
        }
    }
}
