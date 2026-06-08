using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Services.Implementaciones
{
    public class SeguimientoService : ISeguimientoService
    {
        private readonly ISeguimientoRepository _repo;
        private readonly IEvidenciaService _evidenciaService;
        private readonly INotificationService _notif;
        private readonly ISolicitudRepository _solicitudRepo;
        private readonly IAnalisisVulnerabilidadRepository _analisisRepo;

        private const string AdminCD   = "Administrador de Centro de Datos";
        private const string AdminInf  = "Administrador de Infraestructura";
        private const string AdminVul  = "Administrador de Vulnerabilidades";

        public SeguimientoService(
            ISeguimientoRepository repo,
            IEvidenciaService evidenciaService,
            INotificationService notificationService,
            ISolicitudRepository solicitudRepository,
            IAnalisisVulnerabilidadRepository analisisVulnerabilidadRepository)
        {
            _repo            = repo;
            _evidenciaService = evidenciaService;
            _notif           = notificationService;
            _solicitudRepo   = solicitudRepository;
            _analisisRepo    = analisisVulnerabilidadRepository;
        }

        public Task<IEnumerable<Seguimiento>> GetBySolicitudAsync(long solicitudId)
            => _repo.GetBySolicitudId(solicitudId);

        public Task<Seguimiento?> GetEtapaAsync(long solicitudId, int etapaNumero)
            => _repo.GetByEtapa(solicitudId, etapaNumero);

        public Task<int?> GetEtapaActualAsync(long solicitudId)
            => _repo.GetEtapaActual(solicitudId);

        public async Task<List<Seguimiento>> InicializarEtapasAsync(long solicitudId)
        {
            // Evitar duplicar etapas si ya fueron inicializadas
            var existentes = await _repo.GetBySolicitudId(solicitudId);
            if (existentes.Any())
                return existentes.ToList();

            return await _repo.InicializarEtapas(solicitudId);
        }

        public async Task<Seguimiento?> RegresarEtapaAsync(long solicitudId, int etapaDestino, RegresarEtapaRequest request)
        {
            if (etapaDestino < 1 || etapaDestino > 14)
                throw new ArgumentException("La etapa destino debe estar entre 1 y 14.");

            var etapaActual = await _repo.GetEtapaActual(solicitudId);
            if (etapaActual == -1)
                throw new ArgumentException("No se encontraron etapas para esta solicitud.");

            if (etapaDestino >= etapaActual)
                throw new ArgumentException($"La etapa destino ({etapaDestino}) debe ser menor a la etapa actual ({etapaActual}).");

            // Etapa 11 = evidencias (pruebas_funcionamiento). Al regresar a ≤11 se borran las evidencias
            // para que la dependencia suba nuevas en la siguiente ronda.
            if (etapaDestino <= 11)
                await _evidenciaService.EliminarPorSolicitudYPropositoAsync(solicitudId, "pruebas_funcionamiento");

            return await _repo.RegresarAEtapa(solicitudId, etapaDestino, request.Motivo, request.RechazadoBy);
        }

        public async Task<Seguimiento?> RechazarValidacionEvidenciasAsync(long solicitudId, RechazarEvidenciasRequest request)
        {
            if (request.Evidencias == null || !request.Evidencias.Any())
                throw new ArgumentException("Debe especificar al menos una evidencia rechazada.");

            var etapaActual = await _repo.GetEtapaActual(solicitudId);
            if (etapaActual != 12)
                throw new ArgumentException($"Esta operación solo aplica en etapa 12 (validación de evidencias). Etapa actual: {etapaActual}.");

            await _evidenciaService.RechazarEvidenciasAsync(request.Evidencias, request.RechazadoBy ?? 0);

            // Llama directo al repositorio para no activar el borrado masivo del servicio general
            var resultado = await _repo.RegresarAEtapa(solicitudId, 11, request.ObservacionEtapa, request.RechazadoBy);

            var sol = await _solicitudRepo.GetById(solicitudId);
            if (sol != null)
                await _notif.NotificarUsuarioAsync(sol.CreatedBy, "evidencias_rechazadas",
                    "Evidencias rechazadas",
                    $"Las evidencias de tu solicitud {sol.Folio} requieren correcciones.", sol.Id);

            return resultado;
        }

        public async Task<Seguimiento?> AvanzarEtapaAsync(long solicitudId, int etapaNumero, AvanzarEtapaRequest request)
        {
            var etapa = await _repo.GetByEtapa(solicitudId, etapaNumero);
            if (etapa == null) return null;

            var statusValidos = new[] { "en_proceso", "completado", "rechazado" };
            if (!statusValidos.Contains(request.Status))
                throw new ArgumentException($"Status inválido: {request.Status}. Valores permitidos: {string.Join(", ", statusValidos)}");

            if (etapaNumero > 1)
            {
                var anterior = await _repo.GetByEtapa(solicitudId, etapaNumero - 1);
                if (anterior == null || anterior.Status != "completado")
                    throw new ArgumentException($"La etapa {etapaNumero - 1} debe estar completada antes de avanzar a la etapa {etapaNumero}.");
            }

            if (etapa.FechaInicio == null && request.Status == "en_proceso")
                etapa.FechaInicio = DateTime.UtcNow;

            if (request.Status is "completado" or "rechazado")
                etapa.FechaCompletado = DateTime.UtcNow;

            etapa.Status        = request.Status;
            etapa.Observaciones = request.Observaciones ?? etapa.Observaciones;
            etapa.CompletadoBy  = request.CompletadoBy  ?? etapa.CompletadoBy;

            var resultado = await _repo.Update(etapa);

            if (request.Status == "completado")
            {
                await _repo.ActualizarEtapaActualSolicitud(solicitudId, etapaNumero + 1);

                // Al completar la solicitud de publicación (etapa 13), crear el registro de análisis
                if (etapaNumero == 13)
                {
                    var ronda = await _analisisRepo.GetNextRonda(solicitudId);
                    await _analisisRepo.Create(new AnalisisVulnerabilidades
                    {
                        SolicitudId            = solicitudId,
                        Ronda                  = ronda,
                        SolicitudPublicacionBy = request.CompletadoBy,
                        SolicitudPublicacionAt = DateTime.UtcNow,
                        Estado                 = "pendiente"
                    });
                }
            }

            await EnviarNotificacionesEtapaAsync(solicitudId, etapaNumero, request.Status);

            return resultado;
        }

        private async Task EnviarNotificacionesEtapaAsync(long solicitudId, int etapa, string status)
        {
            var sol = await _solicitudRepo.GetById(solicitudId);
            if (sol == null) return;

            var folio = sol.Folio;
            var owner = sol.CreatedBy;

            if (status == "completado")
            {
                switch (etapa)
                {
                    case 8:
                        await _notif.NotificarPorRolAsync(AdminCD, "subdominio_asignado",
                            "Subdominios asignados",
                            $"La solicitud {folio} tiene subdominios asignados.", sol.Id);
                        break;

                    // Etapa 10 (WAF): la dependencia confirmó la configuración WAF
                    case 10:
                        await _notif.NotificarPorRolAsync(AdminCD, "waf_confirmado",
                            "WAF confirmado por dependencia",
                            $"La dependencia confirmó la configuración WAF de la solicitud {folio}. Lista para cargar evidencias.", sol.Id);
                        await _notif.NotificarPorRolAsync(AdminInf, "waf_confirmado",
                            "WAF confirmado por dependencia",
                            $"La dependencia confirmó la configuración WAF de la solicitud {folio}.", sol.Id);
                        break;

                    // Etapa 11 (evidencias): la dependencia cargó evidencias
                    case 11:
                        await _notif.NotificarPorRolAsync(AdminCD, "evidencias_cargadas",
                            "Evidencias cargadas",
                            $"La dependencia cargó evidencias en la solicitud {folio}.", sol.Id);
                        await _notif.NotificarPorRolAsync(AdminInf, "evidencias_cargadas",
                            "Evidencias cargadas",
                            $"La dependencia cargó evidencias en la solicitud {folio}.", sol.Id);
                        break;

                    case 12:
                        await _notif.NotificarPorRolAsync(AdminVul, "evidencias_aprobadas",
                            "Evidencias aprobadas",
                            $"Las evidencias de la solicitud {folio} han sido aprobadas.", sol.Id);
                        await _notif.NotificarUsuarioAsync(owner, "evidencias_aprobadas",
                            "Evidencias aprobadas",
                            $"Las evidencias de tu solicitud {folio} han sido aprobadas.", sol.Id);
                        break;

                    case 13:
                        await _notif.NotificarPorRolAsync(AdminVul, "solicitud_publicacion",
                            "Solicitud de publicación",
                            $"La solicitud {folio} solicita publicación.", sol.Id);
                        break;

                    case 14:
                        await _notif.NotificarUsuarioAsync(owner, "vulnerabilidades_aprobadas",
                            "Análisis de vulnerabilidades aprobado",
                            $"El análisis de vulnerabilidades de tu solicitud {folio} fue aprobado.", sol.Id);
                        await _notif.NotificarPorRolAsync(AdminInf, "vulnerabilidades_aprobadas",
                            "Vulnerabilidades aprobadas",
                            $"La solicitud {folio} pasó el análisis de vulnerabilidades.", sol.Id);
                        break;
                }
            }
            else if (status == "rechazado" && etapa == 14)
            {
                await _notif.NotificarUsuarioAsync(owner, "vulnerabilidades_rechazadas",
                    "Análisis de vulnerabilidades rechazado",
                    $"El análisis de vulnerabilidades de tu solicitud {folio} requiere solventación.", sol.Id);
            }
        }
    }
}
