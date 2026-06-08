using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("subdominio", Schema = "public")]
    public class Subdominio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("subdominio_id")]
        public int SubdominioId { get; set; }

        [Required]
        [Column("requested_name")]
        [StringLength(200)]
        public string RequestedName { get; set; } = string.Empty;

        [Column("approved_name")]
        [StringLength(200)]
        public string? ApprovedName { get; set; }

        [Column("port")]
        public int? Port { get; set; }

        [Column("ssl_required")]
        public bool SslRequired { get; set; } = false;

        [Required]
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; } = "solicitado"; // solicitado | aprobado | activo | expirado | revocado

        [Column("assigned_at")]
        public DateTime? AssignedAt { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ServerSubdominio> ServerSubdominios { get; set; } = new List<ServerSubdominio>();
    }
}
