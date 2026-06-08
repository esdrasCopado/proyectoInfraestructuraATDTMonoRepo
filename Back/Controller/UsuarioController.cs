using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.Back.DTOs;
using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Services.Interfaces;
using SolicitudServidores.Utilities;
using System.Security.Claims;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _repo;
        private readonly IEmailService _email;

        public UsuarioController(IUsuarioRepository repo, IEmailService email)
        {
            _repo  = repo;
            _email = email;
        }

        /// <summary>Lista usuarios con paginación. Filtro opcional por rol.</summary>
        /// <param name="query">
        /// <b>NumPage</b>: página (1-based). <b>Role</b>: nombre exacto del rol —
        /// "Administrador de Centro de Datos" | "Administrador de Infraestructura" |
        /// "Administrador de Vulnerabilidades" | "Administrador General" | "Dependencia / Cliente".
        /// </param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UsuarioDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] Helpers.QueryUserPaging query)
        {
            var usuarios = await _repo.GetAll(query);
            return Ok(usuarios.Select(MapToDto));
        }

        /// <summary>Devuelve todos los usuarios sin paginación.</summary>
        /// <response code="200">Lista completa de usuarios activos.</response>
        [HttpGet("todos")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllSinPaginacion()
        {
            var usuarios = await _repo.GetAll();
            return Ok(usuarios.Select(MapToDto));
        }

        /// <summary>Devuelve los roles disponibles en el sistema.</summary>
        /// <remarks>Usar al crear un usuario para seleccionar el rol a asignar.</remarks>
        /// <response code="200">Lista de roles con id, nombre y descripción.</response>
        [HttpGet("roles")]
        [ProducesResponseType(typeof(IEnumerable<RolDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _repo.GetRoles());
        }

        /// <summary>Devuelve el perfil del usuario autenticado.</summary>
        /// <remarks>Retorna email, puesto (cargo), número de puesto (numero_empleado), celular (phone) y permisos (rol).</remarks>
        /// <response code="200">Perfil del usuario autenticado.</response>
        /// <response code="401">Token JWT no proporcionado o inválido.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMe()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;
            if (!long.TryParse(claim, out var userId)) return Unauthorized();

            var usuario = await _repo.GetById(userId);
            if (usuario == null) return NotFound();
            return Ok(MapToDto(usuario));
        }

        /// <summary>Devuelve un usuario por su ID.</summary>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long id)
        {
            var usuario = await _repo.GetById(id);
            if (usuario == null) return NotFound();
            return Ok(MapToDto(usuario));
        }

        /// <summary>Crea un nuevo usuario y devuelve la contraseña temporal generada.</summary>
        /// <remarks>
        /// La contraseña se genera automáticamente. La respuesta siempre incluye <c>passwordTemporal</c>
        /// para que el administrador pueda compartirla manualmente si el correo no llega.
        /// El campo <c>correoEnviado</c> indica si el envío fue exitoso.
        /// El usuario deberá cambiar la contraseña al primer ingreso (<c>mustChangePassword = true</c>).
        /// </remarks>
        /// <response code="201">Usuario creado. La respuesta incluye la contraseña temporal y el estado del correo.</response>
        /// <response code="400">Nombre, apellidos o correo faltantes.</response>
        /// <response code="409">Ya existe un usuario con ese correo.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CreateUsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellidos) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Nombre, apellidos y correo son requeridos.");
            }

            if (await _repo.ExistsUsuario(request.Email.Trim()))
                return Conflict("Ya existe un usuario con ese correo.");

            var passwordTemporal = _repo.GenerateNewPassword();

            var usuario = new Usuario
            {
                Nombre             = request.Nombre.Trim(),
                Apellidos          = request.Apellidos.Trim(),
                Email              = request.Email.Trim().ToLower(),
                PasswordHash       = Encriptar.EncriptarSHA256(passwordTemporal),
                RoleId             = request.RoleId,
                DependencyId       = request.DependencyId,
                NumeroEmpleado     = request.NumeroEmpleado?.Trim(),
                Cargo              = request.Cargo?.Trim(),
                Phone              = request.Phone?.Trim(),
                Activo             = true,
                MustChangePassword = true
            };

            var creado = await _repo.Create(usuario);

            string? advertenciaCorreo = null;
            bool correoEnviado = false;

            try
            {
                await _email.SendCredencialesAsync(
                    creado.Email,
                    $"{creado.Nombre} {creado.Apellidos}",
                    passwordTemporal);
                correoEnviado = true;
            }
            catch (Exception ex)
            {
                advertenciaCorreo = ex.Message;
            }

            var response = new CreateUsuarioResponse
            {
                Usuario           = MapToDto(creado),
                PasswordTemporal  = passwordTemporal,
                CorreoEnviado     = correoEnviado,
                AdvertenciaCorreo = advertenciaCorreo
            };

            return CreatedAtAction(nameof(GetById), new { id = creado.Id }, response);
        }

        /// <summary>Actualiza los datos de un usuario. Todos los campos son opcionales.</summary>
        /// <remarks>Si se envía <c>password</c>, se reemplaza el hash actual. No resetea <c>mustChangePassword</c>.</remarks>
        /// <response code="200">Usuario actualizado.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUsuarioRequest request)
        {
            var existente = await _repo.GetById(id);
            if (existente == null) return NotFound();

            existente.Nombre         = request.Nombre?.Trim()         ?? existente.Nombre;
            existente.Apellidos      = request.Apellidos?.Trim()      ?? existente.Apellidos;
            existente.Email          = request.Email?.Trim().ToLower() ?? existente.Email;
            existente.RoleId         = request.RoleId                 ?? existente.RoleId;
            existente.DependencyId   = request.DependencyId           ?? existente.DependencyId;
            existente.NumeroEmpleado = request.NumeroEmpleado?.Trim() ?? existente.NumeroEmpleado;
            existente.Cargo          = request.Cargo?.Trim()          ?? existente.Cargo;
            existente.Phone          = request.Phone?.Trim()          ?? existente.Phone;
            existente.Activo         = request.Activo                 ?? existente.Activo;

            if (!string.IsNullOrWhiteSpace(request.Password))
                existente.PasswordHash = Encriptar.EncriptarSHA256(request.Password);

            var actualizado = await _repo.Update(existente);
            if (actualizado == null) return NotFound();

            return Ok(MapToDto(actualizado));
        }

        /// <summary>Elimina (soft delete) un usuario por su ID.</summary>
        /// <response code="200">Usuario eliminado.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            var eliminado = await _repo.Delete(id);
            if (eliminado == null) return NotFound();
            return Ok(MapToDto(eliminado));
        }

        /// <summary>Cambia la contraseña de un usuario y resetea la bandera de cambio obligatorio.</summary>
        /// <remarks>
        /// Usado tanto para el cambio obligatorio al primer ingreso como para cambios posteriores.
        /// Al ejecutarse exitosamente, <c>mustChangePassword</c> queda en <c>false</c>.
        /// Requiere token JWT válido.
        /// </remarks>
        /// <response code="200">Contraseña actualizada. <c>mustChangePassword</c> = false.</response>
        /// <response code="400">Contraseña vacía.</response>
        /// <response code="401">Token no proporcionado o inválido.</response>
        /// <response code="404">Usuario no encontrado.</response>
        [HttpPatch("{id}/password")]
        [Authorize]
        [ProducesResponseType(typeof(UsuarioDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword(long id, [FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("La contraseña no puede estar vacía.");

            var actualizado = await _repo.ChangePassword(id, Encriptar.EncriptarSHA256(request.Password), resetMustChange: true);
            if (actualizado == null) return NotFound();
            return Ok(MapToDto(actualizado));
        }

        private static UsuarioDTO MapToDto(Usuario u) => new()
        {
            Id                 = u.Id,
            Nombre             = u.Nombre,
            Apellidos          = u.Apellidos,
            RoleId             = u.RoleId,
            RolNombre          = u.Rol?.Nombre ?? string.Empty,
            DependencyId       = u.DependencyId,
            Email              = u.Email,
            NumeroEmpleado     = u.NumeroEmpleado,
            Cargo              = u.Cargo,
            Phone              = u.Phone,
            Activo             = u.Activo,
            MustChangePassword = u.MustChangePassword,
            LastLoginAt        = u.LastLoginAt,
            CreatedAt          = u.CreatedAt
        };
    }

    public record ChangePasswordRequest(string Password);
}
