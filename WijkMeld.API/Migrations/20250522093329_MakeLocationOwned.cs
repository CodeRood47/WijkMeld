using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WijkMeld.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeLocationOwned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Incidents_Locations_LocationId",
                table: "Incidents");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_LocationId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Incidents");

            migrationBuilder.AddColumn<string>(
                name: "Location_Address",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Lat",
                table: "Incidents",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Location_Long",
                table: "Incidents",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location_Address",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Location_Lat",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Location_Long",
                table: "Incidents");

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Lat = table.Column<double>(type: "float", nullable: false),
                    Long = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_LocationId",
                table: "Incidents",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Incidents_Locations_LocationId",
                table: "Incidents",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
