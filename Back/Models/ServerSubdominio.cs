using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("server_subdominio", Schema = "public")]
    public class ServerSubdominio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("server_subdominio_id")]
        public int ServerSubdominioId { get; set; }

        [Required]
        [Column("subdominio_id")]
        public int SubdominioId { get; set; }

        [ForeignKey(nameof(SubdominioId))]
        public virtual Subdominio? Subdominio { get; set; }

        [Required]
        [Column("server_id")]
        public long ServerId { get; set; }

        [ForeignKey(nameof(ServerId))]
        public virtual Servidor? Servidor { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
