using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.Back.DTOs;
using SolicitudServidores.DTOs;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Utilities;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly CrearJWT _crearJWT;

        public AuthController(IUsuarioRepository usuarioRepo, CrearJWT crearJWT)
        {
            _usuarioRepo = usuarioRepo;
            _crearJWT = crearJWT;
        }

        /// <summary>Autentica al usuario y devuelve un token JWT.</summary>
        /// <remarks>
        /// Si el campo <c>user.mustChangePassword</c> de la respuesta es <c>true</c>,
        /// el frontend debe redirigir al usuario a cambiar su contraseña antes de permitir el acceso.
        /// El token tiene una validez de 60 minutos.
        /// </remarks>
        /// <response code="200">Login exitoso. Devuelve token JWT y datos del usuario.</response>
        /// <response code="400">Correo o contraseña no proporcionados.</response>
        /// <response code="401">Credenciales inválidas o cuenta desactivada.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Correo y contraseña son requeridos.");

            var usuario = await _usuarioRepo.GetByEmail(request.Email);
            if (usuario == null)
                return Unauthorized("Credenciales inválidas.");

            var passwordHash = Encriptar.EncriptarSHA256(request.Password);
            if (usuario.PasswordHash != passwordHash)
                return Unauthorized("Credenciales inválidas.");

            if (!usuario.Activo)
                return Unauthorized("La cuenta está desactivada.");

            var token = _crearJWT.GenerarToken(usuario);

            var userDto = new UsuarioDTO
            {
                Id                 = usuario.Id,
                Nombre             = usuario.Nombre,
                Apellidos          = usuario.Apellidos,
                RoleId             = usuario.RoleId,
                RolNombre          = usuario.Rol?.Nombre ?? string.Empty,
                DependencyId       = usuario.DependencyId,
                Email              = usuario.Email,
                NumeroEmpleado     = usuario.NumeroEmpleado,
                Cargo              = usuario.Cargo,
                Phone              = usuario.Phone,
                Activo             = usuario.Activo,
                MustChangePassword = usuario.MustChangePassword,
                LastLoginAt        = usuario.LastLoginAt,
                CreatedAt          = usuario.CreatedAt
            };

            return Ok(new LoginResponse { Token = token, User = userDto });
        }
    }
}
