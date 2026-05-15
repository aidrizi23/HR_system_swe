using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class UniqueEmailPreferenceUserType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailPreferences_UserId",
                table: "EmailPreferences");

            migrationBuilder.CreateIndex(
                name: "IX_EmailPreferences_UserId_NotificationType",
                table: "EmailPreferences",
                columns: new[] { "UserId", "NotificationType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailPreferences_UserId_NotificationType",
                table: "EmailPreferences");

            migrationBuilder.CreateIndex(
                name: "IX_EmailPreferences_UserId",
                table: "EmailPreferences",
                column: "UserId");
        }
    }
}
