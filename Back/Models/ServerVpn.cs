using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("server_vpn", Schema = "public")]
    public class ServerVpn
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("server_vpn_id")]
        public int ServerVpnId { get; set; }

        [Required]
        [Column("vpn_id")]
        public int VpnId { get; set; }

        [ForeignKey(nameof(VpnId))]
        public virtual VPN? Vpn { get; set; }

        [Required]
        [Column("server_id")]
        public long ServerId { get; set; }

        [ForeignKey(nameof(ServerId))]
        public virtual Servidor? Servidor { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
