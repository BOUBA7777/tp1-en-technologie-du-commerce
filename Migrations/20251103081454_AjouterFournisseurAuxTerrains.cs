using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TP1.Migrations
{
    /// <inheritdoc />
    public partial class AjouterFournisseurAuxTerrains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FournisseurId",
                table: "Terrains",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Terrains_FournisseurId",
                table: "Terrains",
                column: "FournisseurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Terrains_AspNetUsers_FournisseurId",
                table: "Terrains",
                column: "FournisseurId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Terrains_AspNetUsers_FournisseurId",
                table: "Terrains");

            migrationBuilder.DropIndex(
                name: "IX_Terrains_FournisseurId",
                table: "Terrains");

            migrationBuilder.DropColumn(
                name: "FournisseurId",
                table: "Terrains");
        }
    }
}
