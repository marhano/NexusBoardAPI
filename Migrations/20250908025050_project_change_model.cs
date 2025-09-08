using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusBoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class project_change_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Users_CreateById",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Projects_ProjectId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProjectId1",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreateById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectId1",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Admins",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "CreateBy",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Admins",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "Projects");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId1",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProjectId1",
                table: "Users",
                column: "ProjectId1");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreateById",
                table: "Projects",
                column: "CreateById");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Users_CreateById",
                table: "Projects",
                column: "CreateById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Projects_ProjectId1",
                table: "Users",
                column: "ProjectId1",
                principalTable: "Projects",
                principalColumn: "Id");
        }
    }
}
