using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolicitudServidores.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitudFkToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "solicitud_id",
                schema: "public",
                table: "notifications",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_solicitud_id",
                schema: "public",
                table: "notifications",
                column: "solicitud_id");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_solicitud_solicitud_id",
                schema: "public",
                table: "notifications",
                column: "solicitud_id",
                principalSchema: "public",
                principalTable: "solicitud",
                principalColumn: "solicitud_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_solicitud_solicitud_id",
                schema: "public",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_notifications_solicitud_id",
                schema: "public",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "solicitud_id",
                schema: "public",
                table: "notifications");
        }
    }
}
