using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("waf_config", Schema = "public")]
    public class WafConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("waf_config_id")]
        public long Id { get; set; }

        // 1:1 con server
        [Required]
        [Column("server_id")]
        public long Id_Servidor { get; set; }

        [ForeignKey(nameof(Id_Servidor))]
        public virtual Servidor? Servidor { get; set; }

        // true cuando la dependencia marca la configuración
        [Column("configurado")]
        public bool Configurado { get; set; } = false;

        [Column("reglas_aplicadas", TypeName = "text")]
        public string? ReglasAplicadas { get; set; }

        // Usuario de la dependencia que configuró
        [Column("configured_by")]
        public long? ConfiguredBy { get; set; }

        [ForeignKey(nameof(ConfiguredBy))]
        public virtual Usuario? ConfiguradoPor { get; set; }

        [Column("configured_at")]
        public DateTime? ConfiguredAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
