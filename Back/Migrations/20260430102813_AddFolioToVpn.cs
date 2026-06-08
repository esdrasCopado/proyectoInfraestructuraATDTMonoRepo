using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudServidores.Migrations
{
    /// <inheritdoc />
    public partial class AddFolioToVpn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "folio",
                schema: "public",
                table: "vpn",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "folio",
                schema: "public",
                table: "vpn");
        }
    }
}
