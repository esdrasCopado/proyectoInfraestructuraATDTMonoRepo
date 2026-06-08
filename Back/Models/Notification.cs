using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("notifications", Schema = "public")]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("notification_id")]
        public long NotificationId { get; set; }

        // ── Destinatario ──────────────────────────────────────────────────────

        [Required]
        [Column("recipient_user_id")]
        public long RecipientUserId { get; set; }

        [ForeignKey(nameof(RecipientUserId))]
        public virtual Usuario? Destinatario { get; set; }

        // NULL si fue generada automáticamente por el sistema
        [Column("sender_user_id")]
        public long? SenderUserId { get; set; }

        [ForeignKey(nameof(SenderUserId))]
        public virtual Usuario? Remitente { get; set; }

        // ── Tipo (evento que la disparó) ──────────────────────────────────────

        // subdominio_asignado | evidencias_cargadas | evidencias_aprobadas |
        // evidencias_rechazadas | solicitud_publicacion | vulnerabilidades_aprobadas |
        // vulnerabilidades_rechazadas | publicar_servidor | etc.
        [Required]
        [Column("tipo")]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        // ── Solicitud relacionada ─────────────────────────────────────────────

        [Column("solicitud_id")]
        public long? SolicitudId { get; set; }

        [ForeignKey(nameof(SolicitudId))]
        public virtual Solicitud? Solicitud { get; set; }

        // ── Entidad relacionada (FK polimórfica) ──────────────────────────────

        // evidencia | analisis_vulnerabilidades | vpn | subdominio
        [Column("entity_type")]
        [StringLength(30)]
        public string? EntityType { get; set; }

        [Column("entity_id")]
        public long? EntityId { get; set; }

        // ── Contenido ─────────────────────────────────────────────────────────

        [Required]
        [Column("titulo")]
        [StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required]
        [Column("mensaje", TypeName = "text")]
        public string Mensaje { get; set; } = string.Empty;

        // ── Estado ────────────────────────────────────────────────────────────

        [Column("leida")]
        public bool Leida { get; set; } = false;

        [Column("leida_at")]
        public DateTime? LeidaAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
