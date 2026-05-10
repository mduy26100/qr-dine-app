using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Toppings_ToppingGroupId",
                schema: "catalog",
                table: "Toppings");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                schema: "catalog",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Toppings_ToppingGroupId",
                schema: "catalog",
                table: "Toppings",
                column: "ToppingGroupId")
                .Annotation("SqlServer:Include", new[] { "Name", "Price", "DisplayOrder", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductToppingGroups_ProductId",
                schema: "catalog",
                table: "ProductToppingGroups",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "catalog",
                table: "Products",
                column: "CategoryId")
                .Annotation("SqlServer:Include", new[] { "Name", "Price", "Description", "ImageUrl", "IsAvailable", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Toppings_ToppingGroupId",
                schema: "catalog",
                table: "Toppings");

            migrationBuilder.DropIndex(
                name: "IX_ProductToppingGroups_ProductId",
                schema: "catalog",
                table: "ProductToppingGroups");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                schema: "catalog",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Toppings_ToppingGroupId",
                schema: "catalog",
                table: "Toppings",
                column: "ToppingGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "catalog",
                table: "Products",
                column: "CategoryId");
        }
    }
}
