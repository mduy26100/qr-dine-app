using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_MerchantId",
                schema: "catalog",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MerchantId_ParentId_DisplayOrder",
                schema: "catalog",
                table: "Categories",
                columns: new[] { "MerchantId", "ParentId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MerchantId_ParentId_Name",
                schema: "catalog",
                table: "Categories",
                columns: new[] { "MerchantId", "ParentId", "Name" },
                unique: true,
                filter: "[ParentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_MerchantId_ParentId_DisplayOrder",
                schema: "catalog",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_MerchantId_ParentId_Name",
                schema: "catalog",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MerchantId",
                schema: "catalog",
                table: "Categories",
                column: "MerchantId");
        }
    }
}
