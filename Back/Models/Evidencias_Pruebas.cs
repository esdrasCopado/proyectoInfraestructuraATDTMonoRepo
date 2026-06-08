using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("evidencia", Schema = "public")]
    public class Evidencia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("evidencia_id")]
        public long Id { get; set; }

        [Required]
        [Column("solicitud_id")]
        public long SolicitudId { get; set; }

        [ForeignKey(nameof(SolicitudId))]
        public virtual Solicitud? Solicitud { get; set; }

        // pruebas_funcionamiento | solventacion_vulnerabilidades
        [Required]
        [Column("proposito")]
        [StringLength(30)]
        public string Proposito { get; set; } = string.Empty;

        // 1 = primera carga; 2, 3... si hubo rechazos previos
        [Column("ronda")]
        public int Ronda { get; set; } = 1;

        [Required]
        [Column("archivo_nombre")]
        [StringLength(255)]
        public string ArchivoNombre { get; set; } = string.Empty;

        [Required]
        [Column("archivo_path")]
        [StringLength(500)]
        public string ArchivoPath { get; set; } = string.Empty;

        [Column("archivo_size_kb")]
        public int? ArchivoSizeKb { get; set; }

        // Usuario de la dependencia que cargó
        [Required]
        [Column("uploaded_by")]
        public long UploadedBy { get; set; }

        [ForeignKey(nameof(UploadedBy))]
        public virtual Usuario? SubidoPor { get; set; }

        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // pendiente | aprobada | rechazada
        [Required]
        [Column("estado_validacion")]
        [StringLength(20)]
        public string EstadoValidacion { get; set; } = "pendiente";

        // Requerido si estado_validacion = rechazada
        [Column("motivo_rechazo", TypeName = "text")]
        public string? MotivoRechazo { get; set; }

        [Column("validated_by")]
        public long? ValidatedBy { get; set; }

        [ForeignKey(nameof(ValidatedBy))]
        public virtual Usuario? ValidadoPor { get; set; }

        [Column("validated_at")]
        public DateTime? ValidatedAt { get; set; }

        // Soft delete
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}
