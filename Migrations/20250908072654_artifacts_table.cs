using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusBoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class artifacts_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectArtifact_Projects_ProjectId",
                table: "ProjectArtifact");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectArtifact",
                table: "ProjectArtifact");

            migrationBuilder.RenameTable(
                name: "ProjectArtifact",
                newName: "ProjectArtifacts");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectArtifact_ProjectId",
                table: "ProjectArtifacts",
                newName: "IX_ProjectArtifacts_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectArtifacts",
                table: "ProjectArtifacts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectArtifacts_Projects_ProjectId",
                table: "ProjectArtifacts",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectArtifacts_Projects_ProjectId",
                table: "ProjectArtifacts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectArtifacts",
                table: "ProjectArtifacts");

            migrationBuilder.RenameTable(
                name: "ProjectArtifacts",
                newName: "ProjectArtifact");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectArtifacts_ProjectId",
                table: "ProjectArtifact",
                newName: "IX_ProjectArtifact_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectArtifact",
                table: "ProjectArtifact",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectArtifact_Projects_ProjectId",
                table: "ProjectArtifact",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
