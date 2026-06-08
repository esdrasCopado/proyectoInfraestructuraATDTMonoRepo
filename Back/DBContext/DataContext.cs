using Microsoft.EntityFrameworkCore;
using SolicitudServidores.Models;

namespace SolicitudServidores.DBContext
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // ── Usuarios y roles ──────────────────────────────────────────────────
        public DbSet<Roles> Roles => Set<Roles>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();

        // ── Dependencias y contactos ──────────────────────────────────────────
        public DbSet<Dependency> Dependencies => Set<Dependency>();
        public DbSet<AdminDepContactInformation> AdminDepContacts => Set<AdminDepContactInformation>();

        // ── Infraestructura ───────────────────────────────────────────────────
        public DbSet<Servidor> Servidores => Set<Servidor>();
        public DbSet<Almacenamiento> Storages => Set<Almacenamiento>();
        public DbSet<VPN> VPNs => Set<VPN>();
        public DbSet<ServerVpn> ServerVpns => Set<ServerVpn>();
        public DbSet<Subdominio> Subdominios => Set<Subdominio>();
        public DbSet<ServerSubdominio> ServerSubdominios => Set<ServerSubdominio>();
        public DbSet<WafConfig> WafConfigs => Set<WafConfig>();

        // ── Flujo de solicitudes ──────────────────────────────────────────────
        public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
        public DbSet<Carta> Cartas => Set<Carta>();
        public DbSet<Seguimiento> Seguimientos => Set<Seguimiento>();
        public DbSet<Evidencia> Evidencias => Set<Evidencia>();
        public DbSet<AnalisisVulnerabilidades> AnalisisVulnerabilidades => Set<AnalisisVulnerabilidades>();

        // ── Notificaciones ────────────────────────────────────────────────────
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Roles → Usuarios ──────────────────────────────────────────────
            modelBuilder.Entity<Roles>()
                .HasMany(r => r.Usuarios)
                .WithOne(u => u.Rol)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Roles>()
                .HasIndex(r => r.Nombre)
                .IsUnique();

            // ── Usuarios (self-ref created_by / updated_by) ───────────────────
            modelBuilder.Entity<Usuario>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(u => u.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Usuario>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(u => u.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ── Dependency → Usuarios ─────────────────────────────────────────
            modelBuilder.Entity<Dependency>()
                .HasMany<Usuario>()
                .WithOne(u => u.Dependency)
                .HasForeignKey(u => u.DependencyId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Dependency → AdminDepContacts ─────────────────────────────────
            modelBuilder.Entity<Dependency>()
                .HasMany(d => d.AdminContacts)
                .WithOne(a => a.Dependency)
                .HasForeignKey(a => a.DependencyId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Dependency → Servidores ───────────────────────────────────────
            modelBuilder.Entity<Dependency>()
                .HasMany(d => d.Servidores)
                .WithOne(s => s.Dependency)
                .HasForeignKey(s => s.DependencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Servidor → Almacenamiento ─────────────────────────────────────
            modelBuilder.Entity<Servidor>()
                .HasMany(s => s.Storages)
                .WithOne(a => a.Servidor)
                .HasForeignKey(a => a.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Servidor ↔ VPN (many-to-many via ServerVpn) ───────────────────
            modelBuilder.Entity<ServerVpn>()
                .HasOne(sv => sv.Servidor)
                .WithMany(s => s.ServerVpns)
                .HasForeignKey(sv => sv.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServerVpn>()
                .HasOne(sv => sv.Vpn)
                .WithMany(v => v.ServerVpns)
                .HasForeignKey(sv => sv.VpnId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServerVpn>()
                .HasIndex(sv => new { sv.VpnId, sv.ServerId })
                .IsUnique();

            // ── Servidor ↔ Subdominio (many-to-many via ServerSubdominio) ──────
            modelBuilder.Entity<ServerSubdominio>()
                .HasOne(ss => ss.Servidor)
                .WithMany(s => s.ServerSubdominios)
                .HasForeignKey(ss => ss.ServerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServerSubdominio>()
                .HasOne(ss => ss.Subdominio)
                .WithMany(sub => sub.ServerSubdominios)
                .HasForeignKey(ss => ss.SubdominioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServerSubdominio>()
                .HasIndex(ss => new { ss.SubdominioId, ss.ServerId })
                .IsUnique();

            // ── Servidor → WafConfig (1:1) ────────────────────────────────────
            modelBuilder.Entity<Servidor>()
                .HasOne(s => s.WafConfig)
                .WithOne(w => w.Servidor)
                .HasForeignKey<WafConfig>(w => w.Id_Servidor)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Solicitud → Servidor (1:1, FK en solicitud) ───────────────────
            modelBuilder.Entity<Solicitud>()
                .HasOne(sol => sol.Servidor)
                .WithOne(s => s.Solicitud)
                .HasForeignKey<Solicitud>(sol => sol.ServerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Solicitud>()
                .HasIndex(sol => sol.Folio)
                .IsUnique();

            modelBuilder.Entity<Solicitud>()
                .HasIndex(sol => sol.ServerId)
                .IsUnique();

            // ── Solicitud → Dependency ────────────────────────────────────────
            modelBuilder.Entity<Solicitud>()
                .HasOne(sol => sol.Dependency)
                .WithMany()
                .HasForeignKey(sol => sol.DependencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Solicitud → AdminDepContact ───────────────────────────────────
            modelBuilder.Entity<Solicitud>()
                .HasOne(sol => sol.AdminContact)
                .WithMany()
                .HasForeignKey(sol => sol.AdminContactId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Solicitud → Usuarios (created_by / updated_by) ────────────────
            modelBuilder.Entity<Solicitud>()
                .HasOne(sol => sol.CreadoPor)
                .WithMany()
                .HasForeignKey(sol => sol.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Carta → Solicitud (1:1) ───────────────────────────────────────
            modelBuilder.Entity<Carta>()
                .HasOne(c => c.Solicitud)
                .WithOne(sol => sol.Carta)
                .HasForeignKey<Carta>(c => c.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Carta>()
                .HasIndex(c => c.SolicitudId)
                .IsUnique();

            modelBuilder.Entity<Carta>()
                .HasOne(c => c.CreadoPor)
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Seguimiento → Solicitud ───────────────────────────────────────
            modelBuilder.Entity<Seguimiento>()
                .HasOne(seg => seg.Solicitud)
                .WithMany(sol => sol.Seguimientos)
                .HasForeignKey(seg => seg.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Seguimiento>()
                .HasIndex(seg => new { seg.SolicitudId, seg.EtapaNumero })
                .IsUnique();

            modelBuilder.Entity<Seguimiento>()
                .HasOne(seg => seg.CompletadoPor)
                .WithMany()
                .HasForeignKey(seg => seg.CompletadoBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Evidencia → Solicitud ─────────────────────────────────────────
            modelBuilder.Entity<Evidencia>()
                .HasOne(e => e.Solicitud)
                .WithMany(sol => sol.Evidencias)
                .HasForeignKey(e => e.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Evidencia>()
                .HasOne(e => e.SubidoPor)
                .WithMany()
                .HasForeignKey(e => e.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Evidencia>()
                .HasOne(e => e.ValidadoPor)
                .WithMany()
                .HasForeignKey(e => e.ValidatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ── AnalisisVulnerabilidades → Solicitud ──────────────────────────
            modelBuilder.Entity<AnalisisVulnerabilidades>()
                .HasOne(av => av.Solicitud)
                .WithMany(sol => sol.AnalisisVulnerabilidades)
                .HasForeignKey(av => av.SolicitudId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnalisisVulnerabilidades>()
                .HasIndex(av => new { av.SolicitudId, av.Ronda })
                .IsUnique();

            modelBuilder.Entity<AnalisisVulnerabilidades>()
                .HasOne(av => av.SolicitadoPor)
                .WithMany()
                .HasForeignKey(av => av.SolicitudPublicacionBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnalisisVulnerabilidades>()
                .HasOne(av => av.AnalizadoPor)
                .WithMany()
                .HasForeignKey(av => av.AnalyzedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AnalisisVulnerabilidades>()
                .HasOne(av => av.PublicadoPor)
                .WithMany()
                .HasForeignKey(av => av.PublicadoBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ── WafConfig → Usuarios ──────────────────────────────────────────
            modelBuilder.Entity<WafConfig>()
                .HasOne(w => w.ConfiguradoPor)
                .WithMany()
                .HasForeignKey(w => w.ConfiguredBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Notifications ─────────────────────────────────────────────────
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Destinatario)
                .WithMany()
                .HasForeignKey(n => n.RecipientUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Remitente)
                .WithMany()
                .HasForeignKey(n => n.SenderUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Solicitud)
                .WithMany()
                .HasForeignKey(n => n.SolicitudId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Defaults ──────────────────────────────────────────────────────
            modelBuilder.Entity<Solicitud>()
                .Property(s => s.Estatus)
                .HasDefaultValue("pendiente");

            modelBuilder.Entity<Seguimiento>()
                .Property(s => s.Status)
                .HasDefaultValue("pendiente");

            modelBuilder.Entity<Subdominio>()
                .Property(s => s.Status)
                .HasDefaultValue("solicitado");

            modelBuilder.Entity<Evidencia>()
                .Property(e => e.EstadoValidacion)
                .HasDefaultValue("pendiente");

            modelBuilder.Entity<AnalisisVulnerabilidades>()
                .Property(av => av.Estado)
                .HasDefaultValue("pendiente");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Activo)
                .HasDefaultValue(true);

            // ── Índices compuestos ────────────────────────────────────────────
            modelBuilder.Entity<Solicitud>()
                .HasIndex(sol => sol.DependencyId);

            modelBuilder.Entity<Solicitud>()
                .HasIndex(sol => sol.Estatus);

            modelBuilder.Entity<Solicitud>()
                .HasIndex(sol => sol.CreatedAt);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.RecipientUserId);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.RecipientUserId, n.Leida });

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.CreatedAt);

            // ── Función DB personalizada ──────────────────────────────────────
            var unaccentLowerMethod = typeof(DataContext).GetMethod(nameof(UnaccentLower), new[] { typeof(string) });
            if (unaccentLowerMethod == null)
                throw new InvalidOperationException("No se pudo encontrar el método UnaccentLower.");

            modelBuilder.HasDbFunction(unaccentLowerMethod)
                .HasName("unaccent_lower")
                .HasSchema("public");
        }

        public static string UnaccentLower(string input) => throw new NotImplementedException();
    }
}
