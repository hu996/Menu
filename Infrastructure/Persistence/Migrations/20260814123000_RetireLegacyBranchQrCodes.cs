using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RestaurantMenuPlatform.Infrastructure.Persistence;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations;

/// <summary>
/// Retires legacy branch-level QR rows created before every operational QR
/// was required to resolve to a real RestaurantTable. The rows are retained
/// for audit/history, but an active customer entry point may never be tableless.
/// </summary>
[Migration("20260814123000_RetireLegacyBranchQrCodes")]
[DbContext(typeof(AppDbContext))]
public partial class RetireLegacyBranchQrCodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE [QrCodes] SET [IsActive] = 0, [UpdatedAtUtc] = SYSUTCDATETIME() WHERE [TableId] IS NULL AND [IsActive] = 1;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Legacy rows are intentionally not reactivated on rollback. Doing so
        // would recreate an unsafe tableless public entry point.
    }
}
