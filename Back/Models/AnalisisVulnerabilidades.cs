using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("analisis_vulnerabilidades", Schema = "public")]
    public class AnalisisVulnerabilidades
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("analisis_id")]
        public long AnalisisId { get; set; }

        [Required]
        [Column("solicitud_id")]
        public long SolicitudId { get; set; }

        [ForeignKey(nameof(SolicitudId))]
        public virtual Solicitud? Solicitud { get; set; }

        // La primera es tras RF14; las siguientes tras cargas de solventación
        [Column("ronda")]
        public int Ronda { get; set; } = 1;

        // ── Solicitud de publicación que disparó este análisis (RF14) ─────────

        // Usuario de dependencia que solicitó publicación
        [Column("solicitud_publicacion_by")]
        public long? SolicitudPublicacionBy { get; set; }

        [ForeignKey(nameof(SolicitudPublicacionBy))]
        public virtual Usuario? SolicitadoPor { get; set; }

        [Column("solicitud_publicacion_at")]
        public DateTime? SolicitudPublicacionAt { get; set; }

        // ── Resultado del análisis (RF15) ─────────────────────────────────────

        // pendiente | aprobado | rechazado
        [Required]
        [Column("estado")]
        [StringLength(20)]
        public string Estado { get; set; } = "pendiente";

        [Column("hallazgos", TypeName = "text")]
        public string? Hallazgos { get; set; }

        [Column("recomendaciones", TypeName = "text")]
        public string? Recomendaciones { get; set; }

        // Admin de Vulnerabilidades
        [Column("analyzed_by")]
        public long? AnalyzedBy { get; set; }

        [ForeignKey(nameof(AnalyzedBy))]
        public virtual Usuario? AnalizadoPor { get; set; }

        [Column("analyzed_at")]
        public DateTime? AnalyzedAt { get; set; }

        // ── Publicación (tras aprobación) ─────────────────────────────────────

        // Fecha de publicación efectiva
        [Column("publicado_at")]
        public DateTime? PublicadoAt { get; set; }

        // Admin de Infraestructura que ejecutó la publicación
        [Column("publicado_by")]
        public long? PublicadoBy { get; set; }

        [ForeignKey(nameof(PublicadoBy))]
        public virtual Usuario? PublicadoPor { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
