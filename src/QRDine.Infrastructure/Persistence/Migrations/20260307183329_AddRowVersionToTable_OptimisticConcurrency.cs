using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToTable_OptimisticConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "catalog",
                table: "Tables",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "catalog",
                table: "Tables");
        }
    }
}
