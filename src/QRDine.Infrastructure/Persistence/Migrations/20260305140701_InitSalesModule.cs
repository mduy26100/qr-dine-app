using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitSalesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderCode",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TableName",
                schema: "sales",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "sales",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ToppingsSnapshot",
                schema: "sales",
                table: "OrderItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                schema: "sales",
                table: "Orders",
                column: "OrderCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderCode",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TableName",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ToppingsSnapshot",
                schema: "sales",
                table: "OrderItems");
        }
    }
}
