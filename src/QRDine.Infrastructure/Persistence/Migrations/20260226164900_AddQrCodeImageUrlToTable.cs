using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCodeImageUrlToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrCodeImageUrl",
                schema: "catalog",
                table: "Tables",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCodeImageUrl",
                schema: "catalog",
                table: "Tables");
        }
    }
}
