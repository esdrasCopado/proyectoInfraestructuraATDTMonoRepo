using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("users", Schema = "public")]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("user_id")]
        public long Id { get; set; }

        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Roles? Rol { get; set; }

        // NULL para admins de ATDT; requerido para rol dependencia
        [Column("dependency_id")]
        public int? DependencyId { get; set; }

        [ForeignKey(nameof(DependencyId))]
        public virtual Dependency? Dependency { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("apellidos")]
        [StringLength(150)]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        // Número de empleado institucional (Apartado 6.1 de la carta)
        [Column("numero_empleado")]
        [StringLength(50)]
        public string? NumeroEmpleado { get; set; }

        [Column("cargo")]
        [StringLength(100)]
        public string? Cargo { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        [Column("password_hash")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("must_change_password")]
        public bool MustChangePassword { get; set; } = false;

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("created_by")]
        public long? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}
