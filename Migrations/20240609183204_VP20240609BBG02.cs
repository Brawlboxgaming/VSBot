using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPBot.Migrations
{
    /// <inheritdoc />
    public partial class VP20240609BBG02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Current",
                table: "Submissions");

            migrationBuilder.AlterColumn<string>(
                name: "RejectionReason",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PollID",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PollID",
                table: "Submissions");

            migrationBuilder.AlterColumn<string>(
                name: "RejectionReason",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Current",
                table: "Submissions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
