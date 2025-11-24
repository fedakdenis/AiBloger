using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiBloger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsItemStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "NewsItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "NewsItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ScrapedContent",
                table: "NewsItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "NewsItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NewsItems_Status",
                table: "NewsItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_NewsItems_Status_CreatedAt",
                table: "NewsItems",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NewsItems_Status",
                table: "NewsItems");

            migrationBuilder.DropIndex(
                name: "IX_NewsItems_Status_CreatedAt",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "ScrapedContent",
                table: "NewsItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "NewsItems");
        }
    }
}
