using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("seguimiento", Schema = "public")]
    public class Seguimiento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("seguimiento_id")]
        public long SeguimientoId { get; set; }

        [Required]
        [Column("solicitud_id")]
        public long SolicitudId { get; set; }

        [ForeignKey(nameof(SolicitudId))]
        public virtual Solicitud? Solicitud { get; set; }

        // 1 a 14 (ver etapas canónicas abajo)
        [Required]
        [Column("etapa_numero")]
        public int EtapaNumero { get; set; }

        [Required]
        [Column("etapa_nombre")]
        [StringLength(50)]
        public string EtapaNombre { get; set; } = string.Empty;

        // pendiente | en_proceso | completado | rechazado
        [Required]
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "pendiente";

        [Column("fecha_inicio")]
        public DateTime? FechaInicio { get; set; }

        [Column("fecha_completado")]
        public DateTime? FechaCompletado { get; set; }

        [Column("completado_by")]
        public long? CompletadoBy { get; set; }

        [ForeignKey(nameof(CompletadoBy))]
        public virtual Usuario? CompletadoPor { get; set; }

        [Column("observaciones", TypeName = "text")]
        public string? Observaciones { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /*
         * Etapas canónicas:
         *  1  carta_responsiva       (RF02)
         *  2  validacion_recursos    (RF03)
         *  3  creacion_servidor      (RF04)
         *  4  comunicaciones         (RF05)
         *  5  parches                (RF06)
         *  6  xdr_agente             (RF07)
         *  7  vpn                    (RF08)
         *  8  subdominio             (RF09)
         *  9  credenciales           (RF10)
         * 10  waf                    (RF11)
         * 11  evidencias             (RF12)
         * 12  validacion_evidencias  (RF13)
         * 13  solicitud_publicacion  (RF14)
         * 14  vulnerabilidades       (RF15)
         */
    }
}
