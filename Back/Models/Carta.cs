using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("carta", Schema = "public")]
    public class Carta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("carta_id")]
        public long Id { get; set; }

        // 1:1 con solicitud
        [Required]
        [Column("solicitud_id")]
        public long SolicitudId { get; set; }

        [ForeignKey(nameof(SolicitudId))]
        public virtual Solicitud? Solicitud { get; set; }

        [Column("folio_carta")]
        [StringLength(50)]
        public string? FolioCarta { get; set; }

        // ── Firmante de la Dependencia (Apartado 6) ───────────────────────────

        [Column("firmante_dependencia_nombre")]
        [StringLength(150)]
        public string? FirmanteDependenciaNombre { get; set; }

        [Column("firmante_dependencia_puesto")]
        [StringLength(100)]
        public string? FirmanteDependenciaPuesto { get; set; }

        [Column("firmante_dependencia_empleado")]
        [StringLength(50)]
        public string? FirmanteDependenciaEmpleado { get; set; }

        [Column("firma_dependencia_at")]
        public DateTime? FirmaDependenciaAt { get; set; }

        // ── Firmante ATDT ─────────────────────────────────────────────────────

        [Column("firmante_atdt_nombre")]
        [StringLength(150)]
        public string FirmanteAtdtNombre { get; set; } = "Ing. Germán Félix Moroyoqui";

        [Column("firmante_atdt_puesto")]
        [StringLength(100)]
        public string FirmanteAtdtPuesto { get; set; } = "Director Centro de Datos";

        [Column("firma_atdt_at")]
        public DateTime? FirmaAtdtAt { get; set; }

        // ── Documento PDF (RF10) ──────────────────────────────────────────────

        [Column("pdf_path")]
        [StringLength(500)]
        public string? PdfPath { get; set; }

        [Column("pdf_generated_at")]
        public DateTime? PdfGeneratedAt { get; set; }

        // ── Auditoría ─────────────────────────────────────────────────────────

        [Required]
        [Column("created_by")]
        public long CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual Usuario? CreadoPor { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
