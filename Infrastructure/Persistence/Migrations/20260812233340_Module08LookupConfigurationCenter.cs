using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Module08LookupConfigurationCenter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LookupTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LookupTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LookupTypes_TenantId_Code",
                table: "LookupTypes",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            // Preserve the existing lookup catalog while introducing the controlled type layer.
            // Global values produce global types; tenant values produce tenant types only when
            // no global type with the same code exists.
            migrationBuilder.Sql(@"
INSERT INTO [LookupTypes]
    ([Id], [IsGlobal], [Code], [NameEn], [NameAr], [Description], [IsActive], [SortOrder], [CreatedAtUtc], [UpdatedAtUtc], [TenantId])
SELECT NEWID(), CAST(1 AS bit), [Type], [Type], NULL, N'Migrated from the existing lookup catalog.', CAST(1 AS bit), MIN([SortOrder]), SYSUTCDATETIME(), NULL, '00000000-0000-0000-0000-000000000000'
FROM [LookupValues] AS value
WHERE value.[IsGlobal] = 1
  AND value.[TenantId] = '00000000-0000-0000-0000-000000000000'
  AND NOT EXISTS (
      SELECT 1 FROM [LookupTypes] AS existing
      WHERE existing.[IsGlobal] = 1
        AND existing.[TenantId] = '00000000-0000-0000-0000-000000000000'
        AND existing.[Code] = value.[Type])
GROUP BY [Type];

INSERT INTO [LookupTypes]
    ([Id], [IsGlobal], [Code], [NameEn], [NameAr], [Description], [IsActive], [SortOrder], [CreatedAtUtc], [UpdatedAtUtc], [TenantId])
SELECT NEWID(), CAST(0 AS bit), value.[Type], value.[Type], NULL, N'Migrated from the existing lookup catalog.', CAST(1 AS bit), MIN(value.[SortOrder]), SYSUTCDATETIME(), NULL, value.[TenantId]
FROM [LookupValues] AS value
WHERE value.[IsGlobal] = 0
  AND NOT EXISTS (
      SELECT 1 FROM [LookupTypes] AS existing
      WHERE existing.[Code] = value.[Type]
        AND (existing.[IsGlobal] = 1 OR existing.[TenantId] = value.[TenantId]))
GROUP BY value.[TenantId], value.[Type];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LookupTypes");
        }
    }
}
