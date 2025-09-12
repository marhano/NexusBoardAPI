using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusBoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class threads_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectThread_Projects_ProjectId",
                table: "ProjectThread");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectThreadMessage_ProjectThread_ThreadId",
                table: "ProjectThreadMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectThread",
                table: "ProjectThread");

            migrationBuilder.RenameTable(
                name: "ProjectThread",
                newName: "ProjectThreads");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectThread_ProjectId",
                table: "ProjectThreads",
                newName: "IX_ProjectThreads_ProjectId");

            migrationBuilder.AddColumn<string>(
                name: "Body",
                table: "ProjectThreads",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectThreads",
                table: "ProjectThreads",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectThreadMessage_ProjectThreads_ThreadId",
                table: "ProjectThreadMessage",
                column: "ThreadId",
                principalTable: "ProjectThreads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectThreads_Projects_ProjectId",
                table: "ProjectThreads",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectThreadMessage_ProjectThreads_ThreadId",
                table: "ProjectThreadMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectThreads_Projects_ProjectId",
                table: "ProjectThreads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectThreads",
                table: "ProjectThreads");

            migrationBuilder.DropColumn(
                name: "Body",
                table: "ProjectThreads");

            migrationBuilder.RenameTable(
                name: "ProjectThreads",
                newName: "ProjectThread");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectThreads_ProjectId",
                table: "ProjectThread",
                newName: "IX_ProjectThread_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectThread",
                table: "ProjectThread",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectThread_Projects_ProjectId",
                table: "ProjectThread",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectThreadMessage_ProjectThread_ThreadId",
                table: "ProjectThreadMessage",
                column: "ThreadId",
                principalTable: "ProjectThread",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
