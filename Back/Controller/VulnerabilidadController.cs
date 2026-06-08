using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Controllers
{
    /// <summary>
    /// Gestión de análisis de vulnerabilidades (RF14-RF15).
    /// El registro se crea automáticamente cuando la etapa 13 (solicitud_publicacion) se completa.
    /// </summary>
    [ApiController]
    [Route("api/vulnerabilidad")]
    [Authorize]
    public class VulnerabilidadController : ControllerBase
    {
        private readonly IAnalisisVulnerabilidadRepository _repo;

        public VulnerabilidadController(IAnalisisVulnerabilidadRepository repo)
        {
            _repo = repo;
        }

        /// <summary>Devuelve todos los análisis de vulnerabilidades de una solicitud, ordenados por ronda.</summary>
        /// <param name="solicitudId">ID de la solicitud.</param>
        /// <response code="200">Lista de análisis (puede estar vacía si la solicitud no llegó a etapa 13).</response>
        /// <response code="401">Token JWT no proporcionado.</response>
        [HttpGet("solicitud/{solicitudId}")]
        [ProducesResponseType(typeof(List<AnalisisVulnerabilidadDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBySolicitud(long solicitudId)
        {
            var lista = await _repo.GetBySolicitudId(solicitudId);
            return Ok(lista.Select(MapToDto));
        }

        /// <summary>Devuelve el último (más reciente) análisis de vulnerabilidades de una solicitud.</summary>
        /// <param name="solicitudId">ID de la solicitud.</param>
        /// <response code="200">Último análisis encontrado.</response>
        /// <response code="401">Token JWT no proporcionado.</response>
        /// <response code="404">La solicitud no tiene análisis de vulnerabilidades (aún no alcanzó etapa 13).</response>
        [HttpGet("solicitud/{solicitudId}/ultimo")]
        [ProducesResponseType(typeof(AnalisisVulnerabilidadDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUltimoBySolicitud(long solicitudId)
        {
            var analisis = await _repo.GetUltimaPorSolicitud(solicitudId);
            if (analisis == null) return NotFound();
            return Ok(MapToDto(analisis));
        }

        /// <summary>Devuelve un análisis de vulnerabilidades por su ID.</summary>
        /// <param name="analisisId">ID del análisis.</param>
        /// <response code="200">Análisis encontrado.</response>
        /// <response code="401">Token JWT no proporcionado.</response>
        /// <response code="404">Análisis no encontrado.</response>
        [HttpGet("{analisisId}")]
        [ProducesResponseType(typeof(AnalisisVulnerabilidadDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long analisisId)
        {
            var analisis = await _repo.GetById(analisisId);
            if (analisis == null) return NotFound();
            return Ok(MapToDto(analisis));
        }

        /// <summary>
        /// Registra el resultado del análisis de vulnerabilidades (aprobado / rechazado).
        /// </summary>
        /// <remarks>
        /// Solo el Administrador de Vulnerabilidades puede ejecutar esta acción.
        /// <br/>
        /// - Si se <b>aprueba</b>: el flujo continúa a publicación (etapa 14 puede completarse).
        /// - Si se <b>rechaza</b>: la dependencia debe solventar las observaciones; la solicitud regresa a etapa 13.
        /// <br/>
        /// El campo <c>estado</c> acepta únicamente <c>aprobado</c> o <c>rechazado</c>.
        /// </remarks>
        /// <param name="analisisId">ID del análisis a actualizar.</param>
        /// <param name="request">Resultado, hallazgos, recomendaciones y ID del analista.</param>
        /// <response code="200">Análisis actualizado.</response>
        /// <response code="400">Estado inválido, o el análisis ya fue resuelto (no es pendiente).</response>
        /// <response code="401">Token JWT no proporcionado.</response>
        /// <response code="403">El usuario no tiene el rol de Administrador de Vulnerabilidades.</response>
        /// <response code="404">Análisis no encontrado.</response>
        [HttpPatch("{analisisId}")]
        [Authorize(Roles = "Administrador de Vulnerabilidades,Administrador General")]
        [ProducesResponseType(typeof(AnalisisVulnerabilidadDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SubmitAnalisis(long analisisId, [FromBody] SubmitAnalisisRequest request)
        {
            var estadosValidos = new[] { "aprobado", "rechazado" };
            if (!estadosValidos.Contains(request.Estado))
                return BadRequest($"Estado inválido: '{request.Estado}'. Valores permitidos: aprobado, rechazado.");

            var analisis = await _repo.GetById(analisisId);
            if (analisis == null) return NotFound();

            if (analisis.Estado != "pendiente")
                return BadRequest($"Este análisis ya fue resuelto (estado: {analisis.Estado}). Crea uno nuevo desde la etapa 13.");

            analisis.Estado          = request.Estado;
            analisis.Hallazgos       = request.Hallazgos;
            analisis.Recomendaciones = request.Recomendaciones;
            analisis.AnalyzedBy      = request.AnalyzedBy;
            analisis.AnalyzedAt      = DateTime.UtcNow;

            var actualizado = await _repo.Update(analisis);
            return Ok(MapToDto(actualizado!));
        }

        private static AnalisisVulnerabilidadDTO MapToDto(AnalisisVulnerabilidades a) => new()
        {
            AnalisisId             = a.AnalisisId,
            SolicitudId            = a.SolicitudId,
            Folio                  = a.Solicitud?.Folio,
            Ronda                  = a.Ronda,
            Estado                 = a.Estado,
            Hallazgos              = a.Hallazgos,
            Recomendaciones        = a.Recomendaciones,
            SolicitadoPorNombre    = a.SolicitadoPor != null
                ? $"{a.SolicitadoPor.Nombre} {a.SolicitadoPor.Apellidos}"
                : null,
            SolicitudPublicacionAt = a.SolicitudPublicacionAt,
            AnalizadoPorNombre     = a.AnalizadoPor != null
                ? $"{a.AnalizadoPor.Nombre} {a.AnalizadoPor.Apellidos}"
                : null,
            AnalyzedAt             = a.AnalyzedAt,
            PublicadoAt            = a.PublicadoAt,
            CreatedAt              = a.CreatedAt,
        };
    }
}
