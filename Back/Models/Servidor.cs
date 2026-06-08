using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudServidores.Models
{
    [Table("servidor", Schema = "public")]
    public class Servidor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public long Id { get; set; }

        // dependency_id es requerido en el nuevo esquema
        [Required]
        [Column("dependency_id")]
        public int DependencyId { get; set; }

        [ForeignKey(nameof(DependencyId))]
        public virtual Dependency? Dependency { get; set; }

        // Navegación inversa: la FK está en solicitud.server_id
        public virtual Solicitud? Solicitud { get; set; }

        [Column("Estado")]
        [StringLength(120)]
        public string Estado { get; set; } = "Pendiente";

        [Column("expiracion", TypeName = "date")]
        public DateTime? Expiracion { get; set; }

        [Column("hostname")]
        [StringLength(200)]
        public string Hostname { get; set; } = string.Empty;

        [Column("ip", TypeName = "text")]
        public string? Ip { get; set; }

        [Column("tipo_uso")]
        [StringLength(120)]
        public string TipoUso { get; set; } = "Interno";

        [Column("funcion")]
        [StringLength(300)]
        public string Funcion { get; set; } = string.Empty;

        [Column("sistemaOperativo")]
        [StringLength(300)]
        public string SistemaOperativo { get; set; } = string.Empty;

        [Column("requiere_llave_licencia")]
        public bool RequiereLlaveLicencia { get; set; }

        [Column("llaveOS")]
        [StringLength(300)]
        public string? LlaveOS { get; set; }

        [Column("nucleos")]
        public int Nucleos { get; set; } = 2;

        [Column("ram")]
        public int Ram { get; set; } = 8;

        [Column("almacenamiento")]
        public int Almacenamiento { get; set; } = 100;

        [Column("descripcion", TypeName = "text")]
        public string? Descripcion { get; set; }

        [Column("plantilla_recursos")]
        [StringLength(120)]
        public string PlantillaRecursos { get; set; } = "General";

        [Column("etapa_operativa")]
        [StringLength(120)]
        public string EtapaOperativa { get; set; } = "Provisionamiento";

        [Column("responsable_infraestructura")]
        [StringLength(150)]
        public string? ResponsableInfraestructura { get; set; }

        [Column("usuario_ultima_actualizacion")]
        [StringLength(150)]
        public string? UsuarioUltimaActualizacion { get; set; }

        [Column("fecha_ultima_actualizacion", TypeName = "date")]
        public DateTime? FechaUltimaActualizacion { get; set; }

        [Column("fecha_asignacion_ip", TypeName = "date")]
        public DateTime? FechaAsignacionIp { get; set; }

        [Column("tareas_pendientes", TypeName = "text")]
        public string? TareasPendientes { get; set; }

        [Column("observaciones_seguimiento", TypeName = "text")]
        public string? ObservacionesSeguimiento { get; set; }

        [Column("etapa_vulnerabilidades")]
        [StringLength(120)]
        public string? EtapaVulnerabilidades { get; set; }

        [Column("requiere_revision_anual")]
        public bool RequiereRevisionAnual { get; set; } = true;

        [Column("ultima_revision_anual", TypeName = "date")]
        public DateTime? UltimaRevisionAnual { get; set; }

        [Column("comunicacion_validada")]
        public bool ComunicacionValidada { get; set; }

        [Column("fecha_validacion_comunicacion", TypeName = "date")]
        public DateTime? FechaValidacionComunicacion { get; set; }

        [Column("usuario_validacion_comunicacion")]
        [StringLength(150)]
        public string? UsuarioValidacionComunicacion { get; set; }

        [Column("parches_aplicados")]
        public bool ParchesAplicados { get; set; }

        [Column("fecha_parches", TypeName = "date")]
        public DateTime? FechaParches { get; set; }

        [Column("usuario_parches")]
        [StringLength(150)]
        public string? UsuarioParches { get; set; }

        [Column("xdr_instalado")]
        public bool XdrInstalado { get; set; }

        [Column("fecha_xdr", TypeName = "date")]
        public DateTime? FechaXdr { get; set; }

        [Column("usuario_xdr")]
        [StringLength(150)]
        public string? UsuarioXdr { get; set; }

        [Column("credenciales_entregadas")]
        public bool CredencialesEntregadas { get; set; }

        [Column("fecha_entrega_credenciales", TypeName = "date")]
        public DateTime? FechaEntregaCredenciales { get; set; }

        [Column("usuario_credenciales")]
        [StringLength(150)]
        public string? UsuarioCredenciales { get; set; }

        [Column("waf_configurado")]
        public bool WafConfigurado { get; set; }

        [Column("fecha_configuracion_waf", TypeName = "date")]
        public DateTime? FechaConfiguracionWaf { get; set; }

        [Column("usuario_waf")]
        [StringLength(150)]
        public string? UsuarioWaf { get; set; }

        [Column("evidencia_validada")]
        public bool EvidenciaValidada { get; set; }

        [Column("fecha_validacion_evidencia", TypeName = "date")]
        public DateTime? FechaValidacionEvidencia { get; set; }

        [Column("usuario_validacion_evidencia")]
        [StringLength(150)]
        public string? UsuarioValidacionEvidencia { get; set; }

        [Column("solicitud_publicacion")]
        public bool SolicitudPublicacion { get; set; }

        [Column("fecha_publicacion", TypeName = "date")]
        public DateTime? FechaPublicacion { get; set; }

        [Column("usuario_publicacion")]
        [StringLength(150)]
        public string? UsuarioPublicacion { get; set; }

        [Column("fecha_vulnerabilidades", TypeName = "date")]
        public DateTime? FechaVulnerabilidades { get; set; }

        [Column("usuario_vulnerabilidades")]
        [StringLength(150)]
        public string? UsuarioVulnerabilidades { get; set; }

        public virtual ICollection<ServerVpn> ServerVpns { get; set; } = new List<ServerVpn>();
        public virtual ICollection<ServerSubdominio> ServerSubdominios { get; set; } = new List<ServerSubdominio>();
        public virtual ICollection<Almacenamiento> Storages { get; set; } = new List<Almacenamiento>();
        public virtual WafConfig? WafConfig { get; set; }
    }
}
