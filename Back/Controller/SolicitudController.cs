using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.DTOs;
using SolicitudServidores.Services.Interfaces;
using System.Security.Claims;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/solicitud")]
    [Authorize]
    public class SolicitudController : ControllerBase
    {
        private readonly ISolicitudService _service;
        private readonly IPdfService _pdfService;

        public SolicitudController(ISolicitudService service, IPdfService pdfService)
        {
            _service    = service;
            _pdfService = pdfService;
        }

        // ── Consultas ─────────────────────────────────────────────────────────

        /// <summary>
        /// Lista solicitudes. Dependencia / Cliente solo ve las propias.
        /// Con ?pagina=N aplica paginación de `cantidad` por página.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pagina   = 0,
            [FromQuery] int cantidad = 20)
        {
            if (EsDependencia())
            {
                var userId = ObtenerUserId();
                if (userId == null) return Unauthorized();
                return Ok(await _service.GetByCreatedByAsync(userId.Value));
            }

            return Ok(await _service.GetAllAsync(pagina, cantidad));
        }

        /// <summary>Dependencia / Cliente solo puede ver sus propias solicitudes.</summary>
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var sol = await _service.GetByIdAsync(id);
            if (sol == null) return NotFound();

            if (EsDependencia())
            {
                var userId = ObtenerUserId();
                if (sol.CreatedBy != userId)
                    return Forbid();
            }

            return Ok(sol);
        }

        [HttpGet("folio/{folio}")]
        public async Task<IActionResult> GetByFolio(string folio)
        {
            var sol = await _service.GetByFolioAsync(folio);
            if (sol == null) return NotFound();

            if (EsDependencia())
            {
                var userId = ObtenerUserId();
                if (sol.CreatedBy != userId)
                    return Forbid();
            }

            return Ok(sol);
        }

        /// <summary>Solo administradores pueden listar solicitudes por dependencia.</summary>
        [HttpGet("dependencia/{dependencyId:int}")]
        [Authorize(Roles = "Administrador General,Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades")]
        public async Task<IActionResult> GetByDependencia(int dependencyId)
            => Ok(await _service.GetByDependencyAsync(dependencyId));

        /// <summary>Solo administradores pueden filtrar por estatus.</summary>
        [HttpGet("estatus/{estatus}")]
        [Authorize(Roles = "Administrador General,Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades")]
        public async Task<IActionResult> GetByEstatus(string estatus)
            => Ok(await _service.GetByEstatusAsync(estatus));

        /// <summary>
        /// Dashboard de solicitudes. Dependencia / Cliente solo ve las propias.
        /// Admins ven todas.
        /// </summary>
        [HttpGet("dashboard/resumen")]
        public async Task<IActionResult> GetDashboard()
        {
            if (EsDependencia())
            {
                var userId = ObtenerUserId();
                if (userId == null) return Unauthorized();
                return Ok(await _service.GetDashboardByUserAsync(userId.Value));
            }

            return Ok(await _service.GetDashboardAsync());
        }

        // ── Mutaciones ────────────────────────────────────────────────────────

        /// <summary>Crea solicitud, servidor, discos, VPNs, subdominios y carta en una sola operación transaccional.</summary>
        [HttpPost("completa")]
        public async Task<IActionResult> CreateCompleta([FromBody] CreateSolicitudCompletaRequest request)
        {
            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var creada = await _service.CrearCompletaAsync(request, userId.Value);
                return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "VALIDATION_FAILED", message = ex.Message });
            }
        }

        /// <summary>Crea una solicitud directa (sin carta responsiva). El folio se genera automáticamente.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSolicitudRequest request)
        {
            var userId = ObtenerUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var creada = await _service.CreateAsync(request, userId.Value);
                return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "VALIDATION_FAILED", message = ex.Message });
            }
        }

        /// <summary>Actualización parcial de los campos técnicos de la solicitud.</summary>
        [HttpPatch("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateSolicitudRequest request)
        {
            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            var actualizada = await _service.UpdateAsync(id, request, userId.Value);
            if (actualizada == null) return NotFound();
            return Ok(actualizada);
        }

        /// <summary>RF04 — Cambia el estatus del flujo.</summary>
        [HttpPatch("{id:long}/estatus")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador de Vulnerabilidades,Administrador General")]
        public async Task<IActionResult> ActualizarEstatus(long id, [FromBody] ActualizarEstatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Estatus))
                return BadRequest("El campo 'estatus' es requerido.");

            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var actualizada = await _service.ActualizarEstatusAsync(id, request, userId.Value);
                if (actualizada == null) return NotFound();
                return Ok(actualizada);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "VALIDATION_FAILED", message = ex.Message });
            }
        }

        /// <summary>RF04 — Vincula un servidor aprovisionado a la solicitud.</summary>
        [HttpPatch("{solicitudId:long}/servidor/{serverId:long}")]
        [Authorize(Roles = "Administrador de Infraestructura,Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> AsignarServidor(long solicitudId, long serverId)
        {
            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            var actualizada = await _service.AsignarServidorAsync(solicitudId, serverId, userId.Value);
            if (actualizada == null) return NotFound();
            return Ok(actualizada);
        }

        /// <summary>
        /// Genera y descarga el PDF final de la solicitud.
        /// Disponible a partir de que la solicitud alcanza la etapa 15 (completada).
        /// </summary>
        /// <response code="200">Archivo PDF de la solicitud.</response>
        /// <response code="400">La solicitud aún no ha sido completada (etapa_actual menor a 15).</response>
        /// <response code="404">Solicitud no encontrada.</response>
        [HttpGet("{id:long}/pdf")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DescargarPdf(long id)
        {
            var sol = await _service.GetByIdAsync(id);
            if (sol == null) return NotFound();

            if (sol.EtapaActual < 15)
                return BadRequest(new { error = $"La solicitud aún no está completada. Etapa actual: {sol.EtapaActual}." });

            var bytes = _pdfService.GenerarSolicitudPdf(sol);
            return File(bytes, "application/pdf", $"solicitud_{sol.Folio}.pdf");
        }

        /// <summary>Soft delete — marca la solicitud como eliminada sin borrarla de la base de datos.</summary>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Administrador General")]
        public async Task<IActionResult> Delete(long id)
        {
            var eliminada = await _service.SoftDeleteAsync(id);
            if (eliminada == null) return NotFound();
            return Ok(eliminada);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private long? ObtenerUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;
            return long.TryParse(claim, out var id) ? id : null;
        }

        private bool EsDependencia() => User.IsInRole("Dependencia / Cliente");
    }
}
