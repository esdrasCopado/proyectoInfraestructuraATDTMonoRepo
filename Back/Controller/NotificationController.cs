using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.Back.DTOs;
using SolicitudServidores.DTOs;
using SolicitudServidores.Services.Interfaces;
using System.Security.Claims;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        private static readonly HashSet<string> _rolesAdmin = new(StringComparer.OrdinalIgnoreCase)
        {
            "Administrador de Centro de Datos",
            "Administrador de Infraestructura",
            "Administrador de Vulnerabilidades",
            "Administrador General",
        };

        /// <summary>
        /// Devuelve las notificaciones del usuario autenticado.
        /// Si se proporciona <paramref name="rol"/>, devuelve las notificaciones de todos los usuarios
        /// del rol indicado (requiere rol de administrador).
        /// </summary>
        /// <param name="leida">Filtro opcional: true = leídas, false = no leídas, omitir = todas.</param>
        /// <param name="rol">
        /// Nombre de rol para consultar notificaciones de todos sus usuarios.
        /// Solo disponible para roles de administrador.
        /// Valores válidos: "Administrador de Centro de Datos" | "Administrador de Infraestructura" |
        /// "Administrador de Vulnerabilidades" | "Administrador General" | "Dependencia / Cliente".
        /// </param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMias([FromQuery] bool? leida = null, [FromQuery] string? rol = null)
        {
            if (!string.IsNullOrWhiteSpace(rol))
            {
                var callerRole = User.FindFirst(ClaimTypes.Role)?.Value
                              ?? User.FindFirst("rol")?.Value
                              ?? User.FindFirst("role")?.Value;

                if (callerRole == null || !_rolesAdmin.Contains(callerRole))
                    return Forbid();

                return Ok(await _service.GetByRolAsync(rol.Trim(), leida));
            }

            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _service.GetByRecipientAsync(userId.Value, leida));
        }

        /// <summary>Devuelve el conteo de notificaciones no leídas del usuario autenticado.</summary>
        [HttpGet("no-leidas/count")]
        [ProducesResponseType(typeof(long), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CountNoLeidas()
        {
            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            return Ok(await _service.CountUnreadByRecipientAsync(userId.Value));
        }

        /// <summary>
        /// Marca una notificación como leída o no leída.
        /// Solo el destinatario puede marcar su propia notificación.
        /// </summary>
        [HttpPatch("{id:long}/leer")]
        [ProducesResponseType(typeof(NotificationDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarcarLeida(long id, [FromBody] MarkReadRequest request)
        {
            var userId = ObtenerUserId();
            if (userId == null) return Unauthorized();

            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (existing.RecipientUserId != userId.Value) return Forbid();

            var notification = await _service.MarkAsReadAsync(id, userId.Value, request.Leida);
            return Ok(notification);
        }

        /// <summary>Envía una notificación a un usuario específico por su ID.</summary>
        /// <remarks>
        /// El ID del destinatario se pasa en la ruta. Solo el contenido de la notificación va en el body.
        ///
        /// **Tipos sugeridos para `tipo`:**
        /// - `subdominio_asignado`
        /// - `evidencias_cargadas`
        /// - `evidencias_aprobadas`
        /// - `evidencias_rechazadas`
        /// - `solicitud_publicacion`
        /// - `vulnerabilidades_aprobadas`
        /// - `vulnerabilidades_rechazadas`
        /// </remarks>
        /// <param name="id">ID del usuario destinatario.</param>
        /// <param name="request">Contenido de la notificación.</param>
        /// <returns>La notificación creada.</returns>
        /// <response code="201">Notificación creada correctamente.</response>
        /// <response code="400">Campos requeridos vacíos (tipo, titulo o mensaje).</response>
        /// <response code="401">Token JWT no proporcionado o inválido.</response>
        /// <response code="403">El rol del usuario no tiene permiso para esta operación.</response>
        [HttpPost("usuario/{id:long}")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades,Administrador General,Dependencia / Cliente")]
        [ProducesResponseType(typeof(NotificationDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> NotificarUsuario(long id, [FromBody] NotificarUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Tipo))
                return BadRequest(new { error = "El campo 'tipo' es requerido." });
            if (string.IsNullOrWhiteSpace(request.Titulo))
                return BadRequest(new { error = "El campo 'titulo' es requerido." });
            if (string.IsNullOrWhiteSpace(request.Mensaje))
                return BadRequest(new { error = "El campo 'mensaje' es requerido." });

            var senderId = ObtenerUserId();
            var notification = await _service.CreateAsync(new CreateNotificationRequest
            {
                RecipientUserId = id,
                SenderUserId    = senderId,
                Tipo            = request.Tipo,
                SolicitudId     = request.SolicitudId,
                EntityType      = request.EntityType,
                EntityId        = request.EntityId,
                Titulo          = request.Titulo,
                Mensaje         = request.Mensaje,
            });

            return CreatedAtAction(nameof(GetMias), new { }, notification);
        }

        /// <summary>Crea y envía una notificación a un usuario específico por su ID.</summary>
        /// <remarks>
        /// Disponible para todos los roles autenticados.
        ///
        /// **Tipos sugeridos para `tipo`:**
        /// - `subdominio_asignado` — se asignó un subdominio al servidor
        /// - `evidencias_cargadas` — la dependencia subió evidencias
        /// - `evidencias_aprobadas` — el admin aprobó las evidencias
        /// - `evidencias_rechazadas` — el admin rechazó las evidencias
        /// - `solicitud_publicacion` — se solicitó publicar el servidor
        /// - `vulnerabilidades_aprobadas` — análisis de vulnerabilidades aprobado
        /// - `vulnerabilidades_rechazadas` — análisis de vulnerabilidades rechazado
        ///
        /// Los campos `solicitudId`, `entityType` y `entityId` son opcionales y sirven para
        /// que el frontend pueda navegar directamente al recurso relacionado.
        /// </remarks>
        /// <param name="request">Datos de la notificación a crear.</param>
        /// <returns>La notificación creada.</returns>
        /// <response code="201">Notificación creada correctamente.</response>
        /// <response code="400">Campos requeridos vacíos (tipo, titulo o mensaje).</response>
        /// <response code="401">Token JWT no proporcionado o inválido.</response>
        /// <response code="403">El rol del usuario no tiene permiso para esta operación.</response>
        [HttpPost]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades,Administrador General,Dependencia / Cliente")]
        [ProducesResponseType(typeof(NotificationDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Crear([FromBody] CreateNotificationRequest request)
        {
            if (request.RecipientUserId <= 0)
                return BadRequest(new { error = "El campo 'recipientUserId' es requerido y debe ser mayor a 0. Para enviar a un usuario específico usa POST /api/notifications/usuario/{id}." });

            try
            {
                var notification = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetMias), new { }, notification);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Envía una notificación a todos los usuarios activos de un rol.
        /// </summary>
        /// <remarks>
        /// Roles válidos: "Administrador de Centro de Datos" | "Administrador de Infraestructura" |
        /// "Administrador de Vulnerabilidades" | "Administrador General" | "Dependencia / Cliente".
        /// Se crea una notificación individual por cada usuario del rol.
        /// </remarks>
        /// <response code="204">Notificaciones enviadas correctamente.</response>
        /// <response code="400">Rol, tipo, título o mensaje vacíos.</response>
        [HttpPost("rol")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades,Administrador General")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> NotificarRol([FromBody] NotificarPorRolRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RolNombre))
                return BadRequest(new { error = "El campo 'rolNombre' es requerido." });
            if (string.IsNullOrWhiteSpace(request.Tipo))
                return BadRequest(new { error = "El campo 'tipo' es requerido." });
            if (string.IsNullOrWhiteSpace(request.Titulo))
                return BadRequest(new { error = "El campo 'titulo' es requerido." });
            if (string.IsNullOrWhiteSpace(request.Mensaje))
                return BadRequest(new { error = "El campo 'mensaje' es requerido." });

            var userId = ObtenerUserId();
            await _service.NotificarPorRolAsync(
                request.RolNombre, request.Tipo, request.Titulo, request.Mensaje,
                request.SolicitudId, userId);

            return NoContent();
        }

        /// <summary>
        /// Envía una notificación al usuario Dependencia / Cliente que creó la solicitud indicada.
        /// Solo disponible para roles de administrador.
        /// </summary>
        /// <remarks>
        /// Útil cuando Admin CD o Admin Infraestructura necesita avisar a la dependencia sobre
        /// un evento relacionado con su solicitud (subdominio asignado, evidencias revisadas, etc.).
        /// </remarks>
        /// <response code="204">Notificación enviada correctamente.</response>
        /// <response code="400">Campos requeridos vacíos.</response>
        /// <response code="404">No existe la solicitud con el ID indicado.</response>
        [HttpPost("solicitud/cliente")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador de Infraestructura,Administrador de Vulnerabilidades,Administrador General")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> NotificarClienteDeSolicitud([FromBody] NotificarClienteSolicitudRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Tipo))
                return BadRequest(new { error = "El campo 'tipo' es requerido." });
            if (string.IsNullOrWhiteSpace(request.Titulo))
                return BadRequest(new { error = "El campo 'titulo' es requerido." });
            if (string.IsNullOrWhiteSpace(request.Mensaje))
                return BadRequest(new { error = "El campo 'mensaje' es requerido." });

            try
            {
                var senderId = ObtenerUserId();
                await _service.NotificarClienteDeSolicitudAsync(
                    request.SolicitudId, request.Tipo, request.Titulo, request.Mensaje, senderId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        private long? ObtenerUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;
            return long.TryParse(claim, out var id) ? id : null;
        }
    }
}
