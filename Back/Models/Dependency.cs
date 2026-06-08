using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("dependency", Schema = "public")]
    public class Dependency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("dependency_id")]
        public int DependencyId { get; set; }

        [Column("sector")]
        [StringLength(100)]
        public string? Sector { get; set; }

        [Required]
        [Column("name")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Column("responsable")]
        [StringLength(150)]
        public string? Responsable { get; set; }

        [Column("cargo")]
        [StringLength(100)]
        public string? Cargo { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Column("email")]
        [StringLength(150)]
        public string? Email { get; set; }

        public virtual ICollection<Servidor> Servidores { get; set; } = new List<Servidor>();
        public virtual ICollection<AdminDepContactInformation> AdminContacts { get; set; } = new List<AdminDepContactInformation>();
    }
}
