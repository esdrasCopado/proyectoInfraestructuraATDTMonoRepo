using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudServidores.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoFechasToVpn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado",
                schema: "public",
                table: "vpn",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_asignacion",
                schema: "public",
                table: "vpn",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_expiracion",
                schema: "public",
                table: "vpn",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                schema: "public",
                table: "vpn");

            migrationBuilder.DropColumn(
                name: "fecha_asignacion",
                schema: "public",
                table: "vpn");

            migrationBuilder.DropColumn(
                name: "fecha_expiracion",
                schema: "public",
                table: "vpn");
        }
    }
}
