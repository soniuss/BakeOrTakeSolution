using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.ApiRest.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaRegistroToRecetas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_registro",
                table: "Recetas",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_registro",
                table: "Recetas");
        }
    }
}
