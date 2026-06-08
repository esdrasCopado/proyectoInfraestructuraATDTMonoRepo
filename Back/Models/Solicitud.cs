using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("solicitud", Schema = "public")]
    public class Solicitud
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("solicitud_id")]
        public long Id { get; set; }

        // Folio único asignado al registrar (RF02)
        [Required]
        [Column("folio")]
        [StringLength(50)]
        public string Folio { get; set; } = string.Empty;

        // ── Vínculos ──────────────────────────────────────────────────────────

        [Required]
        [Column("dependency_id")]
        public int DependencyId { get; set; }

        [ForeignKey(nameof(DependencyId))]
        public virtual Dependency? Dependency { get; set; }

        [Column("admin_contact_id")]
        public int? AdminContactId { get; set; }

        [ForeignKey(nameof(AdminContactId))]
        public virtual AdminDepContactInformation? AdminContact { get; set; }

        // NULL hasta aprovisionar en RF04
        [Column("server_id")]
        public long? ServerId { get; set; }

        [ForeignKey(nameof(ServerId))]
        public virtual Servidor? Servidor { get; set; }

        // ── Apartado 2.1: Identificación del servidor ─────────────────────────

        [Required]
        [Column("descripcion_uso", TypeName = "text")]
        public string DescripcionUso { get; set; } = string.Empty;

        [Required]
        [Column("nombre_servidor")]
        [StringLength(200)]
        public string NombreServidor { get; set; } = string.Empty;

        [Column("nombre_aplicacion")]
        [StringLength(200)]
        public string? NombreAplicacion { get; set; }

        // interno | publicado
        [Required]
        [Column("tipo_uso")]
        [StringLength(20)]
        public string TipoUso { get; set; } = string.Empty;

        // ── Apartado 2.2: Planificación ───────────────────────────────────────

        [Column("fecha_arranque_deseada", TypeName = "date")]
        public DateTime? FechaArranqueDeseada { get; set; }

        [Column("vigencia_meses")]
        public int VigenciaMeses { get; set; } = 12;

        [Column("caracteristicas_especiales", TypeName = "text")]
        public string? CaracteristicasEspeciales { get; set; }

        // ── Apartado 3.1/3.2: Requerimiento técnico ──────────────────────────

        // estandar | especifico
        [Required]
        [Column("tipo_requerimiento")]
        [StringLength(20)]
        public string TipoRequerimiento { get; set; } = string.Empty;

        [Column("es_clonacion")]
        public bool EsClonacion { get; set; } = false;

        [Column("ip_servidor_base")]
        [StringLength(45)]
        public string? IpServidorBase { get; set; }

        [Column("nombre_servidor_base")]
        [StringLength(200)]
        public string? NombreServidorBase { get; set; }

        [Column("sistema_operativo")]
        [StringLength(50)]
        public string? SistemaOperativo { get; set; }

        [Required]
        [Column("ram_solicitada_gb")]
        public int RamSolicitadaGb { get; set; }

        [Required]
        [Column("vcpu_solicitado")]
        public int VcpuSolicitado { get; set; }

        [Required]
        [Column("almacenamiento_solicitado_gb")]
        public int AlmacenamientoSolicitadoGb { get; set; }

        // ── Apartado 3.3: Conectividad ────────────────────────────────────────

        [Column("motor_base_datos")]
        [StringLength(100)]
        public string? MotorBaseDatos { get; set; }

        [Column("reglas_firewall", TypeName = "text")]
        public string? ReglasFirewall { get; set; }

        [Column("integraciones_externas", TypeName = "text")]
        public string? IntegracionesExternas { get; set; }

        [Column("conectividad_otras", TypeName = "text")]
        public string? ConectividadOtras { get; set; }

        // ── Estado y auditoría ────────────────────────────────────────────────

        // pendiente | en_validacion | aprovisionado | en_pruebas | publicado | rechazado | finalizado
        [Required]
        [Column("estatus")]
        [StringLength(30)]
        public string Estatus { get; set; } = "pendiente";

        // 1-14, null si aún no se inicializaron las etapas
        [Column("etapa_actual")]
        public int? EtapaActual { get; set; }

        [Required]
        [Column("created_by")]
        public long CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual Usuario? CreadoPor { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        // ── Navegaciones ──────────────────────────────────────────────────────

        public virtual Carta? Carta { get; set; }
        public virtual ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();
        public virtual ICollection<Evidencia> Evidencias { get; set; } = new List<Evidencia>();
        public virtual ICollection<AnalisisVulnerabilidades> AnalisisVulnerabilidades { get; set; } = new List<AnalisisVulnerabilidades>();
    }
}
