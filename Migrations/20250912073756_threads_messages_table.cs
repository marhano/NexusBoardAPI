using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusBoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class threads_messages_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectThreadMessage_ProjectThreads_ThreadId",
                table: "ProjectThreadMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectThreadMessage",
                table: "ProjectThreadMessage");

            migrationBuilder.RenameTable(
                name: "ProjectThreadMessage",
                newName: "ThreadMessages");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectThreadMessage_ThreadId",
                table: "ThreadMessages",
                newName: "IX_ThreadMessages_ThreadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThreadMessages",
                table: "ThreadMessages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ThreadMessages_ProjectThreads_ThreadId",
                table: "ThreadMessages",
                column: "ThreadId",
                principalTable: "ProjectThreads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThreadMessages_ProjectThreads_ThreadId",
                table: "ThreadMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThreadMessages",
                table: "ThreadMessages");

            migrationBuilder.RenameTable(
                name: "ThreadMessages",
                newName: "ProjectThreadMessage");

            migrationBuilder.RenameIndex(
                name: "IX_ThreadMessages_ThreadId",
                table: "ProjectThreadMessage",
                newName: "IX_ProjectThreadMessage_ThreadId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectThreadMessage",
                table: "ProjectThreadMessage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectThreadMessage_ProjectThreads_ThreadId",
                table: "ProjectThreadMessage",
                column: "ThreadId",
                principalTable: "ProjectThreads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
