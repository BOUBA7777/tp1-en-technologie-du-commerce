using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TP1.Migrations
{
    /// <inheritdoc />
    public partial class SimplifierReservationsSansPlaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantite",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "Quantite",
                table: "PanierItems");

            migrationBuilder.DropColumn(
                name: "Capacite",
                table: "Creneaux");

            migrationBuilder.DropColumn(
                name: "PlacesRestantes",
                table: "Creneaux");

            migrationBuilder.AddColumn<bool>(
                name: "EstDisponible",
                table: "Creneaux",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstDisponible",
                table: "Creneaux");

            migrationBuilder.AddColumn<int>(
                name: "Quantite",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantite",
                table: "PanierItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Capacite",
                table: "Creneaux",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlacesRestantes",
                table: "Creneaux",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
