using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("vpn", Schema = "public")]
    public class VPN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("vpn_id")]
        public int VpnId { get; set; }

        [Required]
        [Column("vpn_type")]
        [StringLength(20)]
        public string VpnType { get; set; } = string.Empty; // "dependencia" | "proveedor" | "actualizacion"

        [Required]
        [Column("responsable")]
        [StringLength(150)]
        public string Responsable { get; set; } = string.Empty;

        [Column("cargo")]
        [StringLength(100)]
        public string? Cargo { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Column("email")]
        [StringLength(150)]
        public string? Email { get; set; }

        [Column("vpn_ip")]
        [StringLength(45)]
        public string? VpnIp { get; set; }

        [Column("external_id")]
        [StringLength(50)]
        public string? ExternalId { get; set; }

        // Solo si vpn_type = "proveedor"
        [Column("empresa")]
        [StringLength(150)]
        public string? Empresa { get; set; }

        // 30, 60 o 90 — solo si vpn_type = "proveedor"
        [Column("vigencia_dias")]
        public int? VigenciaDias { get; set; }

        // DGIT## previo si es actualización de usuario de dependencia
        [Column("perfil_anterior")]
        [StringLength(50)]
        public string? PerfilAnterior { get; set; }

        [Column("folio")]
        [StringLength(50)]
        public string? Folio { get; set; }

        [Column("estado")]
        [StringLength(50)]
        public string? Estado { get; set; }

        [Column("fecha_asignacion")]
        public DateOnly? FechaAsignacion { get; set; }

        [Column("fecha_expiracion")]
        public DateOnly? FechaExpiracion { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ServerVpn> ServerVpns { get; set; } = new List<ServerVpn>();
    }
}
