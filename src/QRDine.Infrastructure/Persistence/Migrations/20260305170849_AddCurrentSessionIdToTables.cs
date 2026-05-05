using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentSessionIdToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentSessionId",
                schema: "catalog",
                table: "Tables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_CurrentSessionId",
                schema: "catalog",
                table: "Tables",
                column: "CurrentSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tables_MerchantId_QrCodeToken",
                schema: "catalog",
                table: "Tables",
                columns: new[] { "MerchantId", "QrCodeToken" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tables_CurrentSessionId",
                schema: "catalog",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Tables_MerchantId_QrCodeToken",
                schema: "catalog",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "CurrentSessionId",
                schema: "catalog",
                table: "Tables");
        }
    }
}
