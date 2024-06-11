using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VPBot.Migrations
{
    /// <inheritdoc />
    public partial class VP20240609BBG01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinalScore",
                table: "Submissions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Rejected",
                table: "Submissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubmitterID",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "NewTracks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WikiPage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Video = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewTracks", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewTracks");

            migrationBuilder.DropColumn(
                name: "FinalScore",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Rejected",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubmitterID",
                table: "Submissions");
        }
    }
}
