using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudServidores.Migrations
{
    /// <inheritdoc />
    public partial class AddEtapaActualToSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "etapa_actual",
                schema: "public",
                table: "solicitud",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "etapa_actual",
                schema: "public",
                table: "solicitud");
        }
    }
}
