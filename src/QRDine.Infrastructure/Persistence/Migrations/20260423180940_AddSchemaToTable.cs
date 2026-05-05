using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaToTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SubscriptionCheckouts",
                newName: "SubscriptionCheckouts",
                newSchema: "billing");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "SubscriptionCheckouts",
                schema: "billing",
                newName: "SubscriptionCheckouts");
        }
    }
}
