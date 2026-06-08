namespace SolicitudServidores.Models.Enum
{
    public enum Rol
    {
        ADMINISTRADOR_GENERAL,
        DEPENDENCIA_CLIENTE,
        ADMIN_CENTRO_DATOS,
        ADMIN_INFRAESTRUCTURA,
        ADMIN_VULNERABILIDADES
    }

    public static class RolExtensions
    {
        private static readonly Dictionary<Rol, string> _rolStrings = new Dictionary<Rol, string>
        {
            { Rol.ADMINISTRADOR_GENERAL, "Administrador General" },
            { Rol.DEPENDENCIA_CLIENTE, "Dependencia / Cliente" },
            { Rol.ADMIN_CENTRO_DATOS, "Administrador de Centro de Datos" },
            { Rol.ADMIN_INFRAESTRUCTURA, "Administrador de Infraestructura" },
            { Rol.ADMIN_VULNERABILIDADES, "Administrador de Vulnerabilidades" }
        };

        private static readonly Dictionary<Rol, string> _rolDescripciones = new Dictionary<Rol, string>
        {
            { Rol.ADMINISTRADOR_GENERAL, "Responsable de la gestión global del sistema." },
            { Rol.DEPENDENCIA_CLIENTE, "Personal de las dependencias gubernamentales que genera la solicitud de aprovisionamiento de recursos computacionales y da seguimiento al proceso hasta la puesta en marcha." },
            { Rol.ADMIN_CENTRO_DATOS, "Responsable de validar las solicitudes entrantes, asignar recursos virtuales (RAM, almacenamiento, vCPU, dirección IP) y supervisar el ciclo completo de aprovisionamiento." },
            { Rol.ADMIN_INFRAESTRUCTURA, "Encargado de la configuración de accesos VPN, asignación de subdominios y validación de evidencias de funcionamiento." },
            { Rol.ADMIN_VULNERABILIDADES, "Responsable del análisis de vulnerabilidades previo a la publicación del servidor." }
        };

        private static readonly Dictionary<int, Rol> _rolesMap = new Dictionary<int, Rol>()
        {
            { 1, Rol.ADMINISTRADOR_GENERAL },
            { 2, Rol.DEPENDENCIA_CLIENTE },
            { 3, Rol.ADMIN_CENTRO_DATOS },
            { 4, Rol.ADMIN_INFRAESTRUCTURA },
            { 5, Rol.ADMIN_VULNERABILIDADES }
        };

        public static string ObtenerRolString(this Rol rol)
        {
            return _rolStrings[rol];
        }

        public static string ObtenerDescripcion(this Rol rol)
        {
            return _rolDescripciones[rol];
        }

        public static Rol ObtenerRolPorId(int id)
        {
                return _rolesMap[id];
        }

        public static IEnumerable<string> ObtenerTodosLosRoles()
        {
            return Enum.Rol.GetValues(typeof(Rol)).Cast<Rol>().Select(r => r.ObtenerRolString());
        }

        public static IEnumerable<object> ObtenerTodosLosRolesConDescripcion()
        {
            return System.Enum.GetValues(typeof(Rol)).Cast<Rol>().Select(r => new
            {
                nombre = r.ObtenerRolString(),
                descripcion = r.ObtenerDescripcion()
            });
        }
    }
}
