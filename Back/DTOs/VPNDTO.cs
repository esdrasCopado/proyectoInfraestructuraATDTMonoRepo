
using System.ComponentModel.DataAnnotations.Schema;
namespace AgendaContactosSGD.DTOs.EventoDTOs
    
{
    public class VPNDT
    {
        public long Id { get; set; }
        public long Id_servidor { get; set; }
        public long Id_usuario_responsable { get; set; }
        public string Tipo { get; set; } = null!;
        public DateTime? Fecha_asignacion { get; set; }
        public DateTime? Fecha_Expiracion { get; set; }
        public string? Estado { get; set; }

    }
}
