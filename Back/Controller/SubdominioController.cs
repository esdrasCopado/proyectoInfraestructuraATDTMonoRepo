using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.Back.DTOs;
using SolicitudServidores.DTOs;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Controllers
{
    /// <summary>Gestión de subdominios: actualización de status individual y por lote.</summary>
    [ApiController]
    [Route("api/subdominios")]
    [Produces("application/json")]
    [Authorize(Roles = "Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades,Administrador General")]
    public class SubdominioController : ControllerBase
    {
        private static readonly HashSet<string> _statusValidos = new(StringComparer.OrdinalIgnoreCase)
        {
            "solicitado", "aprobado", "activo", "expirado", "revocado"
        };

        private readonly ISubdominioRepository _repo;

        public SubdominioController(ISubdominioRepository repo)
        {
            _repo = repo;
        }

        /// <summary>Actualiza el status de un subdominio por su ID.</summary>
        /// <param name="id">ID del subdominio a actualizar.</param>
        /// <param name="request">Objeto con el nuevo status.</param>
        /// <returns>Datos del subdominio con el status actualizado.</returns>
        /// <response code="200">Subdominio actualizado correctamente.</response>
        /// <response code="400">Status vacío o valor no permitido.</response>
        /// <response code="401">Token JWT no proporcionado o inválido.</response>
        /// <response code="403">El rol del usuario no tiene permiso para esta operación.</response>
        /// <response code="404">No existe un subdominio con el ID indicado.</response>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(SubdominioStatusItemDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActualizarStatus(int id, [FromBody] ActualizarStatusSubdominioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { error = "El campo 'status' es requerido." });

            if (!_statusValidos.Contains(request.Status))
                return BadRequest(new { error = $"Status inválido. Valores permitidos: {string.Join(", ", _statusValidos)}." });

            var sub = await _repo.UpdateStatus(id, request.Status.ToLower().Trim());
            if (sub == null)
                return NotFound(new { error = $"No existe un subdominio con id {id}." });

            return Ok(new SubdominioStatusItemDTO
            {
                SubdominioId  = sub.SubdominioId,
                RequestedName = sub.RequestedName,
                ApprovedName  = sub.ApprovedName,
                Status        = sub.Status,
                UpdatedAt     = sub.UpdatedAt
            });
        }

        /// <summary>Actualiza el status de varios subdominios en una sola operación.</summary>
        /// <remarks>
        /// Los IDs que no existan son ignorados silenciosamente.
        /// La respuesta incluye únicamente los subdominios que sí fueron actualizados.
        ///
        /// Valores válidos de <c>status</c>: <c>solicitado</c> | <c>aprobado</c> | <c>activo</c> | <c>expirado</c> | <c>revocado</c>
        /// </remarks>
        /// <param name="request">Lista de IDs y el nuevo status a aplicar a todos ellos.</param>
        /// <returns>Conteo y detalle de los subdominios actualizados.</returns>
        /// <response code="200">Operación completada; puede haber 0 actualizados si ningún ID existía.</response>
        /// <response code="400">Lista de IDs vacía, status vacío o valor de status no permitido.</response>
        /// <response code="401">Token JWT no proporcionado o inválido.</response>
        /// <response code="403">El rol del usuario no tiene permiso para esta operación.</response>
        [HttpPatch("batch/status")]
        [ProducesResponseType(typeof(SubdominiosBatchStatusResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ActualizarStatusBatch([FromBody] ActualizarStatusSubdominiosBatchRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0)
                return BadRequest(new { error = "La lista 'ids' no puede estar vacía." });

            if (string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { error = "El campo 'status' es requerido." });

            if (!_statusValidos.Contains(request.Status))
                return BadRequest(new { error = $"Status inválido. Valores permitidos: {string.Join(", ", _statusValidos)}." });

            var actualizados = await _repo.UpdateStatusBatch(request.Ids, request.Status.ToLower().Trim());

            return Ok(new SubdominiosBatchStatusResponseDTO
            {
                Message      = $"{actualizados.Count} subdominio(s) actualizado(s).",
                Actualizados = actualizados.Count,
                Subdominios  = actualizados.Select(s => new SubdominioStatusItemDTO
                {
                    SubdominioId  = s.SubdominioId,
                    RequestedName = s.RequestedName,
                    ApprovedName  = s.ApprovedName,
                    Status        = s.Status,
                    UpdatedAt     = s.UpdatedAt
                })
            });
        }
    }
}
