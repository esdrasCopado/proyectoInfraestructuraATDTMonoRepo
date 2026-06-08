namespace SolicitudServidores.DTOs
{
    public class RolDTO
    {
        public int RoleId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }


    public class UsuarioDTO
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NombreCompleto => $"{Nombre} {Apellidos}".Trim();
        public int RoleId { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public int? DependencyId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? NumeroEmpleado { get; set; }
        public string? Cargo { get; set; }
        public string? Phone { get; set; }
        public bool Activo { get; set; }

        /// <summary>
        /// Indica si el usuario debe cambiar su contraseña al próximo ingreso.
        /// Se activa automáticamente cuando un administrador crea el usuario.
        /// Se desactiva en cuanto el usuario ejecuta PATCH /api/usuario/{id}/password.
        /// </summary>
        public bool MustChangePassword { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
