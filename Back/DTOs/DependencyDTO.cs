namespace SolicitudServidores.DTOs
{
    /// <summary>Datos de una dependencia registrada en el sistema.</summary>
    public class DependencyDTO
    {
        /// <summary>Identificador único de la dependencia.</summary>
        public int DependencyId { get; set; }

        /// <summary>Nombre oficial de la dependencia.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Sector o área al que pertenece (ej. Salud, Educación).</summary>
        public string? Sector { get; set; }

        /// <summary>Nombre del responsable de la dependencia.</summary>
        public string? Responsable { get; set; }

        /// <summary>Cargo del responsable.</summary>
        public string? Cargo { get; set; }

        /// <summary>Teléfono de contacto.</summary>
        public string? Phone { get; set; }

        /// <summary>Correo electrónico de contacto.</summary>
        public string? Email { get; set; }
    }

    /// <summary>Datos para crear una nueva dependencia. Solo <c>name</c> es requerido.</summary>
    public class CreateDependencyRequest
    {
        /// <summary>Nombre oficial de la dependencia. Requerido y único.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Sector o área al que pertenece (ej. Salud, Educación, Seguridad).</summary>
        public string? Sector { get; set; }

        /// <summary>Nombre del responsable principal de la dependencia.</summary>
        public string? Responsable { get; set; }

        /// <summary>Cargo del responsable.</summary>
        public string? Cargo { get; set; }

        /// <summary>Teléfono de contacto.</summary>
        public string? Phone { get; set; }

        /// <summary>Correo electrónico de contacto.</summary>
        public string? Email { get; set; }
    }
}
