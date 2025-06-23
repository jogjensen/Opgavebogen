using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GFVHaveserviceSQL.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTasksNeedToBeCompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TasksNeedToBeCompleted",
                table: "Customers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TasksNeedToBeCompleted",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
