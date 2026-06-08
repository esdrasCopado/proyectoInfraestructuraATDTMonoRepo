using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.DTOs;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/seguimiento")]
    [Authorize]
    public class SeguimientoController : ControllerBase
    {
        private readonly ISeguimientoService _service;

        public SeguimientoController(ISeguimientoService service)
        {
            _service = service;
        }

        /// <summary>Devuelve las 14 etapas de una solicitud ordenadas por número.</summary>
        [HttpGet("solicitud/{solicitudId}")]
        public async Task<IActionResult> GetBySolicitud(long solicitudId)
        {
            return Ok(await _service.GetBySolicitudAsync(solicitudId));
        }

        /// <summary>Devuelve una etapa específica.</summary>
        [HttpGet("solicitud/{solicitudId}/etapa/{etapaNumero}")]
        public async Task<IActionResult> GetEtapa(long solicitudId, int etapaNumero)
        {
            var etapa = await _service.GetEtapaAsync(solicitudId, etapaNumero);
            if (etapa == null) return NotFound();
            return Ok(etapa);
        }

        /// <summary>Devuelve el número de la etapa activa (en_proceso) o la primera pendiente.</summary>
        [HttpGet("solicitud/{solicitudId}/etapa")]
        public async Task<IActionResult> GetEtapaActual(long solicitudId)
        {
            var etapa = await _service.GetEtapaActualAsync(solicitudId);
            //-1 = nulo o no se encontro
            if (etapa == -1) return NotFound();
            return Ok(etapa);
        }

        /// <summary>RF02 — Crea las 14 etapas en estado 'pendiente' al registrar una solicitud.</summary>
        [HttpPost("solicitud/{solicitudId}/inicializar")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> Inicializar(long solicitudId)
        {
            var etapas = await _service.InicializarEtapasAsync(solicitudId);
            return Ok(etapas);
        }

        /// <summary>
        /// Rechaza la validación de evidencias (etapa 12) y regresa a etapa 11.
        /// </summary>
        /// <remarks>
        /// Solo elimina las evidencias indicadas en el listado (las aprobadas se conservan).
        /// Resetea etapas 11 y 12 a 'pendiente' y actualiza <c>etapa_actual = 11</c>.
        /// Solo aplica cuando la solicitud está en etapa 12.
        /// </remarks>
        /// <param name="solicitudId">ID de la solicitud.</param>
        /// <param name="request">IDs de evidencias rechazadas con motivo individual y observación general.</param>
        /// <response code="200">Etapa 11 reseteada. La dependencia puede re-subir evidencias.</response>
        /// <response code="400">No se indicaron evidencias, o la solicitud no está en etapa 12.</response>
        /// <response code="401">Token JWT no proporcionado.</response>
        /// <response code="403">El usuario no tiene rol de Administrador.</response>
        /// <response code="404">Solicitud no encontrada.</response>
        [HttpPatch("solicitud/{solicitudId}/rechazar-evidencias")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador de Infraestructura,Administrador General")]
        [ProducesResponseType(typeof(SolicitudServidores.Models.Seguimiento), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RechazarEvidencias(
            long solicitudId,
            [FromBody] RechazarEvidenciasRequest request)
        {
            try
            {
                var etapa = await _service.RechazarValidacionEvidenciasAsync(solicitudId, request);
                if (etapa == null) return NotFound();
                return Ok(etapa);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Regresa la solicitud a una etapa anterior por rechazo.
        /// </summary>
        /// <remarks>
        /// Resetea a **pendiente** la etapa destino y **todas las etapas posteriores** (limpiando fechas y responsable).
        /// Actualiza <c>etapa_actual</c> en la solicitud a la etapa destino.
        ///
        /// Etapas disponibles (1–14):
        /// 1 carta_responsiva · 2 validacion_recursos · 3 creacion_servidor · 4 comunicaciones ·
        /// 5 parches · 6 xdr_agente · 7 vpn · 8 subdominio · 9 credenciales · 10 waf ·
        /// 11 evidencias · 12 validacion_evidencias · 13 solicitud_publicacion · 14 vulnerabilidades
        ///
        /// **Ejemplo:** Si la solicitud está en etapa 8 y se rechaza, llamar con `etapaDestino=5`
        /// resetea las etapas 5–8 y reinicia el flujo desde parches.
        /// </remarks>
        /// <param name="solicitudId">ID de la solicitud.</param>
        /// <param name="etapaDestino">Número de etapa a la que se regresa (1–14, debe ser menor a la etapa actual).</param>
        /// <param name="request">Motivo del rechazo y usuario que rechaza.</param>
        /// <response code="200">Etapa destino reseteada y devuelta.</response>
        /// <response code="400">Etapa destino inválida o mayor/igual a la etapa actual.</response>
        /// <response code="401">Token JWT no proporcionado.</response>
        /// <response code="403">El usuario no tiene rol de Administrador.</response>
        /// <response code="404">Solicitud o etapas no encontradas.</response>
        [HttpPatch("solicitud/{solicitudId}/regresar/{etapaDestino}")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades,Administrador General")]
        [ProducesResponseType(typeof(SolicitudServidores.Models.Seguimiento), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegresarEtapa(
            long solicitudId,
            int etapaDestino,
            [FromBody] RegresarEtapaRequest request)
        {
            try
            {
                var etapa = await _service.RegresarEtapaAsync(solicitudId, etapaDestino, request);
                if (etapa == null) return NotFound();
                return Ok(etapa);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Avanza el estado de una etapa. Status válidos: en_proceso | completado | rechazado.
        /// Las etapas deben completarse en orden; la etapa N requiere que la N-1 esté completada.
        /// Al completar una etapa, etapa_actual en la solicitud avanza automáticamente.
        /// </summary>
        private static readonly Dictionary<int, string[]> _rolesPorEtapa = new()
        {
            { 1,  new[] { "Administrador de Centro de Datos", "Administrador General" } },
            { 2,  new[] { "Administrador de Centro de Datos", "Administrador General" } },
            { 3,  new[] { "Administrador de Centro de Datos", "Administrador General" } },
            { 4,  new[] { "Administrador de Centro de Datos", "Administrador General" } },
            { 5,  new[] { "Administrador de Centro de Datos", "Administrador General" } },
            { 6,  new[] { "Administrador de Centro de Datos", "Administrador General" } },
            { 7,  new[] { "Administrador de Infraestructura", "Administrador General" } },
            { 8,  new[] { "Administrador de Infraestructura", "Administrador General" } },
            { 9,  new[] { "Administrador de Centro de Datos", "Administrador General" } },
            { 10, new[] { "Dependencia / Cliente", "Administrador General" } },
            { 11, new[] { "Dependencia / Cliente", "Administrador General" } },
            { 12, new[] { "Administrador de Centro de Datos", "Administrador de Infraestructura", "Administrador General" } },
            { 13, new[] { "Dependencia / Cliente", "Administrador General" } },
            { 14, new[] { "Administrador de Vulnerabilidades", "Administrador General" } },
        };

        [HttpPatch("solicitud/{solicitudId}/etapa/{etapaNumero}")]
        public async Task<IActionResult> AvanzarEtapa(
            long solicitudId,
            int etapaNumero,
            [FromBody] AvanzarEtapaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
                return BadRequest("El campo 'status' es requerido.");

            if (!_rolesPorEtapa.TryGetValue(etapaNumero, out var rolesPermitidos))
                return BadRequest($"Etapa {etapaNumero} no válida.");

            if (!rolesPermitidos.Any(r => User.IsInRole(r)))
                return Forbid();

            try
            {
                var etapa = await _service.AvanzarEtapaAsync(solicitudId, etapaNumero, request);
                if (etapa == null) return NotFound();
                return Ok(etapa);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
