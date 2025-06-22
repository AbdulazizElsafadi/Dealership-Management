using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dealership_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedByAdminToPurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessedByAdminId",
                table: "Purchases",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_ProcessedByAdminId",
                table: "Purchases",
                column: "ProcessedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Users_ProcessedByAdminId",
                table: "Purchases",
                column: "ProcessedByAdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Users_ProcessedByAdminId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_ProcessedByAdminId",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "ProcessedByAdminId",
                table: "Purchases");
        }
    }
}
