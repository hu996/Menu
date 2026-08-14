using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantMenuPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Branches",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE Branches
                SET Slug = CASE
                    WHEN LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(Name, ' ', '-'), '&', 'and'), '/', '-'))) = ''
                        THEN 'branch-' + RIGHT(CONVERT(varchar(36), Id), 8)
                    ELSE LOWER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(Name, ' ', '-'), '&', 'and'), '/', '-'))))
                END;

                WITH DuplicateSlugs AS
                (
                    SELECT Id,
                           ROW_NUMBER() OVER (PARTITION BY TenantId, Slug ORDER BY CreatedAtUtc, Id) AS RowNumber
                    FROM Branches
                )
                UPDATE b
                SET Slug = CONCAT(b.Slug, '-', d.RowNumber)
                FROM Branches b
                INNER JOIN DuplicateSlugs d ON d.Id = b.Id
                WHERE d.RowNumber > 1;");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId_Slug",
                table: "Branches",
                columns: new[] { "TenantId", "Slug" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Branches_TenantId_Slug",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Branches");
        }
    }
}
