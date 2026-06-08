using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("storage", Schema = "public")]
    public class Almacenamiento
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("server_id")]
        public long ServerId { get; set; }

        [ForeignKey(nameof(ServerId))]
        public virtual Servidor? Servidor { get; set; }

        [Required]
        [Column("storage_capacity")]
        public int StorageCapacity { get; set; }

        [Column("type")]
        [StringLength(30)]
        public string? Type { get; set; }

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
