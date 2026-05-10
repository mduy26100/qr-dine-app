using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QRDine.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanSnapshotName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlanSnapshotName",
                schema: "billing",
                table: "Transactions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanSnapshotName",
                table: "SubscriptionCheckouts",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanSnapshotName",
                schema: "billing",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PlanSnapshotName",
                table: "SubscriptionCheckouts");
        }
    }
}
