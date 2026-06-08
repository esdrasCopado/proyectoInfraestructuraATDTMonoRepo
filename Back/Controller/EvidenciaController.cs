using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.DTOs;
using SolicitudServidores.Services.Interfaces;
using System.Security.Claims;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/evidencia")]
    [Authorize]
    public class EvidenciaController : ControllerBase
    {
        private readonly IEvidenciaService _service;
        private readonly ISolicitudService _solicitudService;

        public EvidenciaController(IEvidenciaService service, ISolicitudService solicitudService)
        {
            _service          = service;
            _solicitudService = solicitudService;
        }

        /// <summary>Devuelve todas las evidencias de una solicitud.</summary>
        /// <response code="200">Lista de evidencias (puede ser vacía si aún no se han subido).</response>
        /// <response code="404">La solicitud no existe.</response>
        [HttpGet("solicitud/{solicitudId:long}")]
        [ProducesResponseType(typeof(IEnumerable<EvidenciaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBySolicitud(long solicitudId)
        {
            var solicitud = await _solicitudService.GetByIdAsync(solicitudId);
            if (solicitud == null) return NotFound(new { error = $"No existe la solicitud con ID {solicitudId}." });

            return Ok(await _service.GetBySolicitudAsync(solicitudId));
        }

        /// <summary>
        /// Devuelve todas las evidencias de una solicitud incluyendo las eliminadas (historial completo).
        /// Las evidencias con <c>deletedAt</c> distinto de null fueron rechazadas en una ronda anterior.
        /// </summary>
        /// <response code="200">Historial completo de evidencias.</response>
        /// <response code="404">La solicitud no existe.</response>
        [HttpGet("solicitud/{solicitudId:long}/todas")]
        [ProducesResponseType(typeof(IEnumerable<EvidenciaResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTodasBySolicitud(long solicitudId)
        {
            var solicitud = await _solicitudService.GetByIdAsync(solicitudId);
            if (solicitud == null) return NotFound(new { error = $"No existe la solicitud con ID {solicitudId}." });

            return Ok(await _service.GetAllBySolicitudAsync(solicitudId));
        }

        /// <summary>Devuelve una evidencia por su ID.</summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var evidencia = await _service.GetByIdAsync(id);
            if (evidencia == null) return NotFound();
            return Ok(evidencia);
        }

        /// <summary>
        /// Sube un archivo PDF como evidencia de una solicitud.
        /// Propósito: pruebas_funcionamiento | solventacion_vulnerabilidades.
        /// La ronda se calcula automáticamente.
        /// </summary>
        [HttpPost("solicitud/{solicitudId:long}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Subir(
            long solicitudId,
            [FromForm] string proposito,
            IFormFile archivo)
        {
            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            if (archivo == null || archivo.Length == 0)
                return BadRequest("Debes adjuntar un archivo PDF.");

            try
            {
                var evidencia = await _service.SubirAsync(solicitudId, proposito, archivo, userId.Value);
                return CreatedAtAction(nameof(GetById), new { id = evidencia.Id }, evidencia);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Valida una evidencia: aprobada o rechazada.
        /// Si se rechaza, el motivo_rechazo es obligatorio.
        /// </summary>
        [HttpPatch("{id:long}/validar")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> Validar(long id, [FromBody] ValidarEvidenciaRequest request)
        {
            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var evidencia = await _service.ValidarAsync(id, request.EstadoValidacion, request.MotivoRechazo, userId.Value);
                if (evidencia == null) return NotFound();
                return Ok(evidencia);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Descarga el archivo PDF de una evidencia. Requiere autenticación.</summary>
        [HttpGet("{id:long}/descargar")]
        public async Task<IActionResult> Descargar(long id)
        {
            var evidencia = await _service.GetByIdAsync(id);
            if (evidencia == null) return NotFound();

            var rutaFisica = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot", evidencia.ArchivoPath);

            if (!System.IO.File.Exists(rutaFisica))
                return NotFound(new { error = "Archivo no encontrado en el servidor." });

            var stream = System.IO.File.OpenRead(rutaFisica);
            return File(stream, "application/pdf", evidencia.ArchivoNombre);
        }

        /// <summary>Elimina (soft delete) una evidencia.</summary>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> Eliminar(long id)
        {
            var eliminada = await _service.EliminarAsync(id);
            if (eliminada == null) return NotFound();
            return Ok(eliminada);
        }

        private long? ObtenerUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;
            return long.TryParse(claim, out var id) ? id : null;
        }
    }
}
