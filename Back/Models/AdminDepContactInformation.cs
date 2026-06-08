using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("admin_dep_contact_information", Schema = "public")]
    public class AdminDepContactInformation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("admin_contact_id")]
        public int AdminContactId { get; set; }

        [Required]
        [Column("dependency_id")]
        public int DependencyId { get; set; }

        [ForeignKey(nameof(DependencyId))]
        public virtual Dependency? Dependency { get; set; }

        [Column("proveedor")]
        [StringLength(150)]
        public string? Proveedor { get; set; }

        [Required]
        [Column("admin_servidor")]
        [StringLength(150)]
        public string AdminServidor { get; set; } = string.Empty;

        [Column("cargo")]
        [StringLength(100)]
        public string? Cargo { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Column("email")]
        [StringLength(150)]
        public string? Email { get; set; }
    }
}
