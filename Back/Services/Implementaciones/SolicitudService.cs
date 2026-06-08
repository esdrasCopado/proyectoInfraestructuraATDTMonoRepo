using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Services.Implementaciones
{
    public class SolicitudService : ISolicitudService
    {
        private readonly ISolicitudRepository _repo;
        private readonly DataContext _context;
        private readonly ISeguimientoService _seguimiento;

        private static readonly HashSet<string> EstatusValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "pendiente", "en_validacion", "aprovisionado",
            "en_pruebas", "publicado", "rechazado", "finalizado"
        };

        private readonly INotificationService _notif;

        public SolicitudService(ISolicitudRepository repo, DataContext context, ISeguimientoService seguimiento, INotificationService notificationService)
        {
            _repo        = repo;
            _context     = context;
            _seguimiento = seguimiento;
            _notif       = notificationService;
        }

        public async Task<IEnumerable<Solicitud>> GetAllAsync(int pagina = 0, int cantidad = 20)
        {
            if (pagina <= 0)
                return await _repo.GetAll();

            return await _repo.GetAllPaged(pagina, cantidad);
        }

        public Task<Solicitud?> GetByIdAsync(long id) => _repo.GetById(id);

        public Task<Solicitud?> GetByFolioAsync(string folio) => _repo.GetByFolio(folio);

        public Task<IEnumerable<Solicitud>> GetByDependencyAsync(int dependencyId)
            => _repo.GetByDependency(dependencyId);

        public Task<IEnumerable<Solicitud>> GetByEstatusAsync(string estatus)
            => _repo.GetByEstatus(estatus);

        public Task<IEnumerable<Solicitud>> GetByCreatedByAsync(long userId)
            => _repo.GetByCreatedBy(userId);

        public async Task<SolicitudDashboardDto> GetDashboardAsync()
        {
            var todas = (await _repo.GetAll()).ToList();
            return BuildDashboard(todas);
        }

        public async Task<SolicitudDashboardDto> GetDashboardByUserAsync(long userId)
        {
            var propias = (await _repo.GetByCreatedBy(userId)).ToList();
            return BuildDashboard(propias);
        }

        private static SolicitudDashboardDto BuildDashboard(List<Solicitud> solicitudes) =>
            new SolicitudDashboardDto
            {
                Total = solicitudes.Count,
                Solicitudes = solicitudes.Select(s => new SolicitudResumenDto
                {
                    Id            = s.Id,
                    Folio         = s.Folio,
                    Dependencia   = s.Dependency?.Name ?? "Sin dependencia",
                    Estado        = s.Estatus,
                    EtapaActual   = s.EtapaActual,
                    NombreEtapa   = ObtenerNombreEtapa(s.EtapaActual),
                    Actualizacion = s.UpdatedAt,
                })
            };

        private static readonly Dictionary<int, string> _nombresEtapa = new()
        {
            { 1,  "Carta responsiva" },
            { 2,  "Validación de recursos" },
            { 3,  "Creación de servidor" },
            { 4,  "Comunicaciones" },
            { 5,  "Parches" },
            { 6,  "XDR agente" },
            { 7,  "VPN" },
            { 8,  "Subdominio" },
            { 9,  "Credenciales" },
            { 10, "WAF" },
            { 11, "Evidencias" },
            { 12, "Validación de evidencias" },
            { 13, "Solicitud de publicación" },
            { 14, "Vulnerabilidades" },
            { 15, "Completado" },
        };

        private static string? ObtenerNombreEtapa(int? etapa) =>
            etapa.HasValue && _nombresEtapa.TryGetValue(etapa.Value, out var nombre) ? nombre : null;

        public async Task<Solicitud> CreateAsync(CreateSolicitudRequest request, long createdBy)
        {
            ValidarRequest(request);

            var folio = GenerarFolio();

            // Garantizar unicidad del folio (colisión muy improbable pero posible)
            while (await _repo.ExistsFolio(folio))
                folio = GenerarFolio();

            var solicitud = new Solicitud
            {
                Folio                    = folio,
                DependencyId             = request.DependencyId,
                AdminContactId           = request.AdminContactId,
                DescripcionUso           = request.DescripcionUso.Trim(),
                NombreServidor           = request.NombreServidor.Trim(),
                NombreAplicacion         = request.NombreAplicacion?.Trim(),
                TipoUso                  = request.TipoUso.Trim().ToLower(),
                FechaArranqueDeseada     = request.FechaArranqueDeseada,
                VigenciaMeses            = request.VigenciaMeses > 0 ? request.VigenciaMeses : 12,
                CaracteristicasEspeciales = request.CaracteristicasEspeciales?.Trim(),
                TipoRequerimiento        = request.TipoRequerimiento.Trim().ToLower(),
                EsClonacion              = request.EsClonacion,
                IpServidorBase           = request.IpServidorBase?.Trim(),
                NombreServidorBase       = request.NombreServidorBase?.Trim(),
                SistemaOperativo         = request.SistemaOperativo?.Trim(),
                RamSolicitadaGb          = request.RamSolicitadaGb,
                VcpuSolicitado           = request.VcpuSolicitado,
                AlmacenamientoSolicitadoGb = request.AlmacenamientoSolicitadoGb,
                MotorBaseDatos           = request.MotorBaseDatos?.Trim(),
                ReglasFirewall           = request.ReglasFirewall?.Trim(),
                IntegracionesExternas    = request.IntegracionesExternas?.Trim(),
                ConectividadOtras        = request.ConectividadOtras?.Trim(),
                Estatus                  = "pendiente",
                EtapaActual              = 1,
                CreatedBy                = createdBy,
                CreatedAt                = DateTime.UtcNow,
                UpdatedAt                = DateTime.UtcNow,
            };

            var result = await _repo.Create(solicitud);
            await _seguimiento.InicializarEtapasAsync(result.Id);
            await _notif.NotificarPorRolAsync("Administrador de Centro de Datos", "solicitud_nueva",
                "Nueva solicitud recibida",
                $"Se registró la solicitud {result.Folio}.", result.Id);
            return result;
        }

        public async Task<Solicitud?> UpdateAsync(long id, UpdateSolicitudRequest request, long updatedBy)
        {
            var existente = await _repo.GetById(id);
            if (existente == null) return null;

            existente.AdminContactId            = request.AdminContactId           ?? existente.AdminContactId;
            existente.DescripcionUso            = request.DescripcionUso?.Trim()   ?? existente.DescripcionUso;
            existente.NombreServidor            = request.NombreServidor?.Trim()   ?? existente.NombreServidor;
            existente.NombreAplicacion          = request.NombreAplicacion?.Trim() ?? existente.NombreAplicacion;
            existente.TipoUso                   = request.TipoUso?.Trim().ToLower() ?? existente.TipoUso;
            existente.FechaArranqueDeseada      = request.FechaArranqueDeseada     ?? existente.FechaArranqueDeseada;
            existente.VigenciaMeses             = request.VigenciaMeses            ?? existente.VigenciaMeses;
            existente.CaracteristicasEspeciales = request.CaracteristicasEspeciales?.Trim() ?? existente.CaracteristicasEspeciales;
            existente.TipoRequerimiento         = request.TipoRequerimiento?.Trim().ToLower() ?? existente.TipoRequerimiento;
            existente.EsClonacion               = request.EsClonacion              ?? existente.EsClonacion;
            existente.IpServidorBase            = request.IpServidorBase?.Trim()   ?? existente.IpServidorBase;
            existente.NombreServidorBase        = request.NombreServidorBase?.Trim() ?? existente.NombreServidorBase;
            existente.SistemaOperativo          = request.SistemaOperativo?.Trim() ?? existente.SistemaOperativo;
            existente.RamSolicitadaGb           = request.RamSolicitadaGb          ?? existente.RamSolicitadaGb;
            existente.VcpuSolicitado            = request.VcpuSolicitado           ?? existente.VcpuSolicitado;
            existente.AlmacenamientoSolicitadoGb = request.AlmacenamientoSolicitadoGb ?? existente.AlmacenamientoSolicitadoGb;
            existente.MotorBaseDatos            = request.MotorBaseDatos?.Trim()   ?? existente.MotorBaseDatos;
            existente.ReglasFirewall            = request.ReglasFirewall?.Trim()   ?? existente.ReglasFirewall;
            existente.IntegracionesExternas     = request.IntegracionesExternas?.Trim() ?? existente.IntegracionesExternas;
            existente.ConectividadOtras         = request.ConectividadOtras?.Trim() ?? existente.ConectividadOtras;
            existente.UpdatedBy                 = updatedBy;

            return await _repo.Update(existente);
        }

        public async Task<Solicitud?> ActualizarEstatusAsync(long id, ActualizarEstatusRequest request, long updatedBy)
        {
            if (!EstatusValidos.Contains(request.Estatus))
                throw new ArgumentException(
                    $"Estatus inválido: '{request.Estatus}'. Valores permitidos: {string.Join(", ", EstatusValidos)}");

            var existente = await _repo.GetById(id);
            if (existente == null) return null;

            existente.Estatus   = request.Estatus.ToLower();
            existente.UpdatedBy = updatedBy;

            return await _repo.Update(existente);
        }

        public async Task<Solicitud?> AsignarServidorAsync(long solicitudId, long serverId, long updatedBy)
        {
            var existente = await _repo.GetById(solicitudId);
            if (existente == null) return null;

            return await _repo.AsignarServidor(solicitudId, serverId);
        }

        public Task<Solicitud?> SoftDeleteAsync(long id) => _repo.SoftDelete(id);

        public async Task<Solicitud> CrearCompletaAsync(CreateSolicitudCompletaRequest request, long createdBy)
        {
            if (string.IsNullOrWhiteSpace(request.Dependencia))
                throw new ArgumentException("El campo 'dependencia' es requerido.");
            if (request.Servidores == null || request.Servidores.Count == 0)
                throw new ArgumentException("Se requiere al menos un servidor.");

            var srv0 = request.Servidores[0];

            if (string.IsNullOrWhiteSpace(srv0.Hostname))
                throw new ArgumentException("El campo 'hostname' del servidor es requerido.");
            if (srv0.Nucleos < 1)
                throw new ArgumentException("El campo 'nucleos' debe ser >= 1.");
            if (srv0.Ram < 1)
                throw new ArgumentException("El campo 'ram' debe ser >= 1.");

            long createdSolicitudId = 0;
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Dependency: find-or-create por nombre
                var dep = await _context.Dependencies
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == request.Dependencia.Trim().ToLower());

                if (dep == null)
                {
                    dep = new Dependency
                    {
                        Name        = request.Dependencia.Trim(),
                        Sector      = request.Sector?.Trim(),
                        Responsable = request.ResponsableActual?.Trim(),
                        Cargo       = request.CargoResponsable?.Trim(),
                        Phone       = request.TelefonoResponsable?.Trim(),
                        Email       = request.CorreoResponsable?.Trim(),
                    };
                    _context.Dependencies.Add(dep);
                    await _context.SaveChangesAsync();
                }

                // 2. AdminDepContactInformation
                var adminContact = new AdminDepContactInformation
                {
                    DependencyId  = dep.DependencyId,
                    Proveedor     = request.Proveedor?.Trim(),
                    AdminServidor = (request.DependenciaAdmin ?? string.Empty).Trim(),
                    Cargo         = request.CargoAdmin?.Trim(),
                    Phone         = request.TelefonoAdmin?.Trim(),
                    Email         = request.CorreoAdmin?.Trim(),
                };
                _context.AdminDepContacts.Add(adminContact);
                await _context.SaveChangesAsync();

                // 3. Servidor
                var totalAlmacenamiento = srv0.DiscosDuros.Any()
                    ? srv0.DiscosDuros.Sum(d => d.Capacidad)
                    : (srv0.Ram * 10);

                var servidor = new Servidor
                {
                    DependencyId               = dep.DependencyId,
                    Hostname                   = srv0.Hostname.Trim(),
                    TipoUso                    = (srv0.TipoUso ?? "interno").Trim(),
                    Funcion                    = (srv0.Funcion ?? request.Titulo ?? string.Empty).Trim(),
                    Descripcion                = srv0.Descripcion?.Trim(),
                    SistemaOperativo           = (srv0.SistemaOperativo ?? string.Empty).Trim(),
                    RequiereLlaveLicencia      = srv0.RequiereLlaveLicencia,
                    Nucleos                    = srv0.Nucleos,
                    Ram                        = srv0.Ram,
                    Almacenamiento             = totalAlmacenamiento,
                    PlantillaRecursos          = (srv0.PlantillaRecursos ?? "General").Trim(),
                    ResponsableInfraestructura = srv0.ResponsableInfraestructura?.Trim(),
                    UsuarioUltimaActualizacion = request.UsuarioUltimaActualizacion?.Trim(),
                    SolicitudPublicacion       = srv0.SolicitudPublicacion,
                    Estado                     = "Pendiente",
                };
                _context.Servidores.Add(servidor);
                await _context.SaveChangesAsync();

                // 3a. Discos (Almacenamiento)
                foreach (var disco in srv0.DiscosDuros)
                {
                    _context.Storages.Add(new Almacenamiento
                    {
                        ServerId        = servidor.Id,
                        StorageCapacity = disco.Capacidad,
                        Type            = disco.Tipo?.Trim(),
                        Description     = disco.Etiqueta?.Trim(),
                    });
                }

                // 3b. VPNs
                foreach (var vpnDto in srv0.VpNs)
                {
                    var vpn = new VPN
                    {
                        VpnType        = MapearTipoVpn(vpnDto.Tipo),
                        Responsable    = vpnDto.Responsable?.Trim() ?? string.Empty,
                        Cargo          = vpnDto.Cargo?.Trim(),
                        Phone          = vpnDto.Telefono?.Trim(),
                        Email          = vpnDto.Correo?.Trim(),
                        PerfilAnterior = vpnDto.PerfilAnterior?.Trim(),
                        VpnIp          = vpnDto.Ip?.Trim(),
                        Empresa        = vpnDto.Empresa?.Trim(),
                        VigenciaDias   = int.TryParse(vpnDto.Vigencia, out var v) ? v : null,
                        Folio          = !string.IsNullOrWhiteSpace(vpnDto.Folio) ? vpnDto.Folio.Trim() : GenerarFolioVpn(),
                        Estado         = vpnDto.Estado?.Trim(),
                        FechaAsignacion = DateOnly.TryParse(vpnDto.FechaAsignacion, out var fa) ? fa : null,
                        FechaExpiracion = DateOnly.TryParse(vpnDto.FechaExpiracion, out var fe) ? fe : null,
                    };
                    _context.VPNs.Add(vpn);
                    await _context.SaveChangesAsync();

                    _context.ServerVpns.Add(new ServerVpn
                    {
                        VpnId      = vpn.VpnId,
                        ServerId   = servidor.Id,
                        AssignedAt = DateTime.UtcNow,
                    });
                }

                // 3c. Subdominios
                foreach (var subDto in srv0.Subdominios)
                {
                    if (string.IsNullOrWhiteSpace(subDto.NombreUrl)) continue;

                    var sub = new Subdominio
                    {
                        RequestedName = subDto.NombreUrl.Trim(),
                        Port          = int.TryParse(subDto.Puerto, out var p) ? p : null,
                        SslRequired   = subDto.RequiereSSL || srv0.RequiereSSL,
                        Status        = "solicitado",
                    };
                    _context.Subdominios.Add(sub);
                    await _context.SaveChangesAsync();

                    _context.ServerSubdominios.Add(new ServerSubdominio
                    {
                        SubdominioId = sub.SubdominioId,
                        ServerId     = servidor.Id,
                    });
                }

                await _context.SaveChangesAsync();

                // 4. Solicitud
                var folio = GenerarFolio();
                while (await _repo.ExistsFolio(folio))
                    folio = GenerarFolio();

                var solicitud = new Solicitud
                {
                    Folio                      = folio,
                    DependencyId               = dep.DependencyId,
                    AdminContactId             = adminContact.AdminContactId,
                    ServerId                   = servidor.Id,
                    DescripcionUso             = (request.Descripcion ?? request.Titulo ?? string.Empty).Trim(),
                    NombreServidor             = srv0.Hostname.Trim(),
                    NombreAplicacion           = request.Titulo?.Trim(),
                    TipoUso                    = (srv0.TipoUso ?? "interno").Trim().ToLower(),
                    FechaArranqueDeseada       = DateTime.TryParse(request.FechaRequerida, out var fecha) ? fecha : null,
                    VigenciaMeses              = ParsearVigencia(srv0.Vigencia),
                    TipoRequerimiento          = (srv0.PlantillaRecursos ?? "estandar").Trim().ToLower(),
                    EsClonacion                = string.Equals(srv0.Modalidad, "clonacion", StringComparison.OrdinalIgnoreCase),
                    SistemaOperativo           = srv0.SistemaOperativo?.Trim(),
                    RamSolicitadaGb            = srv0.Ram,
                    VcpuSolicitado             = srv0.Nucleos,
                    AlmacenamientoSolicitadoGb = totalAlmacenamiento,
                    MotorBaseDatos             = srv0.MotorBD?.Trim(),
                    ReglasFirewall             = srv0.Puertos?.Trim(),
                    IntegracionesExternas      = srv0.Integraciones?.Trim(),
                    ConectividadOtras          = srv0.OtrasSpecs?.Trim(),
                    Estatus                    = "pendiente",
                    EtapaActual                = 1,
                    CreatedBy                  = createdBy,
                    CreatedAt                  = DateTime.UtcNow,
                    UpdatedAt                  = DateTime.UtcNow,
                };
                _context.Solicitudes.Add(solicitud);
                await _context.SaveChangesAsync();

                // 5. Carta
                var carta = new Carta
                {
                    SolicitudId                 = solicitud.Id,
                    FirmanteDependenciaNombre   = request.Firmante?.Trim(),
                    FirmanteDependenciaPuesto   = request.PuestoFirmante?.Trim(),
                    FirmanteDependenciaEmpleado = request.NumEmpleado?.Trim(),
                    FirmaDependenciaAt          = request.AceptaTerminos ? DateTime.UtcNow : null,
                    CreatedBy                   = createdBy,
                    CreatedAt                   = DateTime.UtcNow,
                    UpdatedAt                   = DateTime.UtcNow,
                };
                _context.Cartas.Add(carta);
                await _context.SaveChangesAsync();

                createdSolicitudId = solicitud.Id;
                await _seguimiento.InicializarEtapasAsync(solicitud.Id);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            var creada = (await _repo.GetById(createdSolicitudId))!;
            await _notif.NotificarPorRolAsync("Administrador de Centro de Datos", "solicitud_nueva",
                "Nueva solicitud recibida",
                $"Se registró la solicitud {creada.Folio}.", creada.Id);
            return creada;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string MapearTipoVpn(string? tipo)
        {
            var t = tipo?.ToLower() ?? string.Empty;
            if (t.Contains("proveedor"))    return "proveedor";
            if (t.Contains("actualizacion") || t.Contains("actualización")) return "actualizacion";
            return "dependencia";
        }

        private static int ParsearVigencia(string? vigencia)
        {
            if (string.IsNullOrWhiteSpace(vigencia)) return 12;
            var num = new string(vigencia.Where(char.IsDigit).ToArray());
            if (!int.TryParse(num, out var n) || n <= 0) return 12;
            return vigencia.ToLower().Contains("año") ? n * 12 : n;
        }

        private static string GenerarFolio()
            => $"SOL-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}";

        private static string GenerarFolioVpn()
            => $"VPN-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        private static void ValidarRequest(CreateSolicitudRequest r)
        {
            if (r.DependencyId <= 0)
                throw new ArgumentException("El campo 'dependencyId' es requerido.");
            if (string.IsNullOrWhiteSpace(r.DescripcionUso))
                throw new ArgumentException("El campo 'descripcionUso' es requerido.");
            if (string.IsNullOrWhiteSpace(r.NombreServidor))
                throw new ArgumentException("El campo 'nombreServidor' es requerido.");
            if (r.TipoUso != "interno" && r.TipoUso != "publicado")
                throw new ArgumentException("El campo 'tipoUso' debe ser 'interno' o 'publicado'.");
            if (r.RamSolicitadaGb < 1)
                throw new ArgumentException("El campo 'ramSolicitadaGb' debe ser >= 1.");
            if (r.VcpuSolicitado < 1)
                throw new ArgumentException("El campo 'vcpuSolicitado' debe ser >= 1.");
            if (r.AlmacenamientoSolicitadoGb < 1)
                throw new ArgumentException("El campo 'almacenamientoSolicitadoGb' debe ser >= 1.");
        }
    }
}
