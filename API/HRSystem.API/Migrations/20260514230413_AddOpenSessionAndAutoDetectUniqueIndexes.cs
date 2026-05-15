using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenSessionAndAutoDetectUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_OpenSession_EmployeeId",
                table: "TimeLogs",
                column: "EmployeeId",
                unique: true,
                filter: "\"EndTime\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeRecords_AutoDetected_Active",
                table: "OvertimeRecords",
                columns: new[] { "EmployeeId", "Date" },
                unique: true,
                filter: "\"Type\" = 1 AND (\"Status\" = 0 OR \"Status\" = 3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeLogs_OpenSession_EmployeeId",
                table: "TimeLogs");

            migrationBuilder.DropIndex(
                name: "IX_OvertimeRecords_AutoDetected_Active",
                table: "OvertimeRecords");
        }
    }
}
