namespace SolicitudServidores.Back.DTOs
{
    // ─── 1.1 Solicitudes por dependencia (Admin CD) ───────────────────────────
    public class Reporte11ItemDto
    {
        public string FolioSolicitud      { get; set; } = string.Empty;
        public string? Sector             { get; set; }
        public string Dependencia         { get; set; } = string.Empty;
        public string Responsable         { get; set; } = string.Empty;
        public string Contacto            { get; set; } = string.Empty;
        public string EstatusProcesamieto { get; set; } = string.Empty;
        public DateTime FechaCreacion     { get; set; }
    }

    // ─── 1.2 Recursos solicitados (Admin CD) ──────────────────────────────────
    public class Reporte12ItemDto
    {
        public string  FolioSolicitud        { get; set; } = string.Empty;
        public string? Sector                { get; set; }
        public string  Dependencia           { get; set; } = string.Empty;
        public string  Responsable           { get; set; } = string.Empty;
        public string  Contacto              { get; set; } = string.Empty;
        public string  EstatusProcesamieto   { get; set; } = string.Empty;
        public string? IpServidor            { get; set; }
        public string? AdministradorServidor { get; set; }
        public string? DescripcionProyecto   { get; set; }
        public string  SistemaOperativo      { get; set; } = string.Empty;
        public int     Vcpu                  { get; set; }
        public int     Ram                   { get; set; }
        public int     Almacenamiento        { get; set; }
        public DateTime FechaCreacion        { get; set; }
    }

    public class Reporte12ResponseDto
    {
        public List<Reporte12ItemDto> Items               { get; set; } = new();
        public int                    TotalVcpu           { get; set; }
        public int                    TotalRam            { get; set; }
        public int                    TotalAlmacenamiento { get; set; }
    }

    // ─── 1.3 Reporte por IP (Admin CD) ────────────────────────────────────────
    public class Reporte13ItemDto
    {
        public string       FolioSolicitud        { get; set; } = string.Empty;
        public string?      Sector                { get; set; }
        public string       Dependencia           { get; set; } = string.Empty;
        public string       Responsable           { get; set; } = string.Empty;
        public string       ContactoResponsable   { get; set; } = string.Empty;
        public string       EstatusProcesamieto   { get; set; } = string.Empty;
        public string?      IpServidor            { get; set; }
        public string?      AdministradorServidor { get; set; }
        public string?      DescripcionProyecto   { get; set; }
        public string       SistemaOperativo      { get; set; } = string.Empty;
        public int          Vcpu                  { get; set; }
        public int          Ram                   { get; set; }
        public int          Almacenamiento        { get; set; }
        public List<string> SubdominiosAprobados  { get; set; } = new();
        public List<string> Vpns                  { get; set; } = new();
    }

    // ─── 2.1 VPN (Admin Infraestructura) ──────────────────────────────────────
    public class Reporte21ItemDto
    {
        public string    FolioSolicitud      { get; set; } = string.Empty;
        public string?   Sector              { get; set; }
        public string    Dependencia         { get; set; } = string.Empty;
        public string?   ResponsableServidor { get; set; }
        public string?   ContactoResponsable { get; set; }
        public string    EstatusProcesamieto { get; set; } = string.Empty;
        public string?   IpServidor          { get; set; }
        public string?   IdentificadorVpn    { get; set; }
        public string?   UsuarioAsignado     { get; set; }
        public DateOnly? FechaCreacionVpn    { get; set; }
        public DateOnly? FechaVencimientoVpn { get; set; }
        public int?      Vigencia            { get; set; }
        public string    TipoVpn             { get; set; } = string.Empty;
    }

    // ─── 2.2 Subdominios (Admin Infraestructura) ──────────────────────────────
    public class Reporte22ItemDto
    {
        public string   FolioSolicitud      { get; set; } = string.Empty;
        public string?  Sector              { get; set; }
        public string   Dependencia         { get; set; } = string.Empty;
        public string?  ResponsableServidor { get; set; }
        public string?  Contacto            { get; set; }
        public string   EstatusProcesamieto { get; set; } = string.Empty;
        public string?  IpServidor          { get; set; }
        public string   SubdominioAprobado  { get; set; } = string.Empty;
        public string?  ProxyAsignado       { get; set; }
        public string?  TipoDespliegue      { get; set; }
        public int?     Puerto              { get; set; }
        public DateTime? FechaAsignacion    { get; set; }
    }

    // ─── 3.1 Resultados de vulnerabilidades (Admin Vulnerabilidades) ──────────
    public class Reporte31ItemDto
    {
        public string    FolioSolicitud            { get; set; } = string.Empty;
        public string?   Sector                    { get; set; }
        public string    Dependencia               { get; set; } = string.Empty;
        public string?   ResponsableServidor       { get; set; }
        public string?   TelefonoContacto          { get; set; }
        public string?   EmailContacto             { get; set; }
        public string    EstatusProcesamieto        { get; set; } = string.Empty;
        public string?   IpServidor                { get; set; }
        public List<string> SubdominiosAprobados   { get; set; } = new();
        public DateTime? FechaSolicitudAnalisis    { get; set; }
        public DateTime? FechaAplicacionPrueba     { get; set; }
        public string?   ResultadoPrueba           { get; set; }
        public string?   Hallazgos                 { get; set; }
        public int       Ronda                     { get; set; }
    }

    // ─── 3.2 Comunicaciones y aplicativos por IP (Admin Vulnerabilidades) ─────
    public class Reporte32ItemDto
    {
        public string    FolioSolicitud          { get; set; } = string.Empty;
        public string?   Sector                  { get; set; }
        public string    Dependencia             { get; set; } = string.Empty;
        public string?   ResponsableServidor     { get; set; }
        public string?   ContactoResponsable     { get; set; }
        public string    EstatusProcesamieto     { get; set; } = string.Empty;
        public string?   IpServidor              { get; set; }
        public List<string> SubdominiosAprobados { get; set; } = new();
        public string?   TipoDespliegue          { get; set; }
        public string?   ReglasFirewall          { get; set; }
        public string?   IntegracionesExternas   { get; set; }
        public string?   Otras                   { get; set; }
    }

    // ─── 4.1 Estatus de solicitudes (Admin General) ───────────────────────────
    public class Reporte41ItemDto
    {
        public string?   Sector                    { get; set; }
        public string    Dependencia               { get; set; } = string.Empty;
        public string    Responsable               { get; set; } = string.Empty;
        public string    EmailContacto             { get; set; } = string.Empty;
        public string?   DescripcionServidor       { get; set; }
        public string?   Ip                        { get; set; }
        public DateTime  FechaSolicitud            { get; set; }
        public string    EstatusProcesamieto        { get; set; } = string.Empty;
        public int?      EtapaActual               { get; set; }
        public string?   NombreEtapaActual         { get; set; }
        public DateTime? FechaProcesamientoEtapa   { get; set; }
        public string?   RolResponsableEtapa       { get; set; }
        public DateTime? FechaPublicacion          { get; set; }
        public string?   TipoDespliegue            { get; set; }
    }

    public class Reporte41ResponseDto
    {
        public List<Reporte41ItemDto> Items           { get; set; } = new();
        public int                    TotalSolicitudes { get; set; }
    }

    // ─── 4.2 Recursos solicitados (Admin General) ─────────────────────────────
    public class Reporte42ItemDto
    {
        public string       FolioSolicitud        { get; set; } = string.Empty;
        public string?      Sector                { get; set; }
        public string       Dependencia           { get; set; } = string.Empty;
        public string       Responsable           { get; set; } = string.Empty;
        public string       Contacto              { get; set; } = string.Empty;
        public string       EstatusProcesamieto   { get; set; } = string.Empty;
        public string?      IpServidor            { get; set; }
        public string?      AdministradorServidor { get; set; }
        public string?      DescripcionProyecto   { get; set; }
        public string       SistemaOperativo      { get; set; } = string.Empty;
        public int          Vcpu                  { get; set; }
        public int          Ram                   { get; set; }
        public int          Almacenamiento        { get; set; }
        public List<string> SubdominiosAprobados  { get; set; } = new();
        public List<string> Vpns                  { get; set; } = new();
        public DateTime     FechaCreacion         { get; set; }
    }

    public class Reporte42ResponseDto
    {
        public List<Reporte42ItemDto> Items               { get; set; } = new();
        public int                    TotalVcpu           { get; set; }
        public int                    TotalRam            { get; set; }
        public int                    TotalAlmacenamiento { get; set; }
        public int                    TotalServidores     { get; set; }
    }
}
