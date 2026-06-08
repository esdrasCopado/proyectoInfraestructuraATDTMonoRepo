
using System.ComponentModel.DataAnnotations.Schema;
namespace AgendaContactosSGD.DTOs.EventoDTOs
    
{
    public class Evidencias_PruebasDTO
    {
        public long Id { get; set; }
        public long Id_servidor { get; set; }
        public long Id_usuario_responsable { get; set; }
        public string Ruta_pdf { get; set; } = null!;
        public DateTime? Fecha{ get; set; }
    }
}
