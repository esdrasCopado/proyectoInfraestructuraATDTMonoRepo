using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.DTOs;
using SolicitudServidores.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/cartas")]
    [Authorize]
    public class CartaController : ControllerBase
    {
        private readonly ICartaService _service;

        private static readonly Regex EmailRegex    = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex TelefonoRegex = new(@"^\(\d{3}\) \d{3}-\d{4}$",    RegexOptions.Compiled);

        private static readonly HashSet<string> TiposUso           = new(StringComparer.OrdinalIgnoreCase) { "interno", "publicado" };
        private static readonly HashSet<string> TiposRequerimiento = new(StringComparer.OrdinalIgnoreCase) { "estandar", "especifico", "especial" };
        private static readonly HashSet<string> Modalidades        = new(StringComparer.OrdinalIgnoreCase) { "nuevo", "renovacion", "clonacion", "serverBase" };
        private static readonly HashSet<string> SistemasOperativos = new(StringComparer.OrdinalIgnoreCase) { "windows", "linux", "otro" };
        private static readonly HashSet<string> TiposDisco         = new(StringComparer.OrdinalIgnoreCase) { "SSD", "HDD", "NVMe" };

        public CartaController(ICartaService service)
        {
            _service = service;
        }

        /// <summary>Lista todas las cartas responsivas.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        /// <summary>Devuelve una carta por su ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var carta = await _service.GetByIdAsync(id);
            if (carta == null) return NotFound();
            return Ok(carta);
        }

        /// <summary>Devuelve la carta asociada a una solicitud.</summary>
        [HttpGet("solicitud/{solicitudId}")]
        public async Task<IActionResult> GetBySolicitud(long solicitudId)
        {
            var carta = await _service.GetBySolicitudAsync(solicitudId);
            if (carta == null) return NotFound();
            return Ok(carta);
        }

        /// <summary>RF02 — Crea una carta responsiva junto con su solicitud y las 14 etapas de seguimiento.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CartaRequestDTO dto)
        {
            var error = Validar(dto);
            if (error != null) return BadRequest(error);

            var createdBy = ObtenerUserId();
            if (createdBy == null)
                return Unauthorized(new { message = "No se pudo identificar al usuario autenticado." });

            var carta = await _service.CreateAsync(dto, createdBy.Value);
            return CreatedAtAction(nameof(GetById), new { id = carta.Id }, new
            {
                cartaId    = carta.Id,
                folioCarta = carta.FolioCarta,
                solicitudId = carta.SolicitudId,
            });
        }

        /// <summary>Registra la firma de la dependencia en la carta.</summary>
        [HttpPatch("{id}/firmar-dependencia")]
        public async Task<IActionResult> FirmarDependencia(long id, [FromBody] FirmaDependenciaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FirmanteNombre))
                return BadRequest("El campo 'firmanteNombre' es requerido.");

            var carta = await _service.FirmarDependenciaAsync(id, request);
            if (carta == null) return NotFound();
            return Ok(carta);
        }

        /// <summary>Registra la firma del ATDT y completa la etapa carta_responsiva.</summary>
        [HttpPatch("{id}/firmar-atdt")]
        [Authorize(Roles = "Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> FirmarAtdt(long id, [FromBody] FirmaAtdtRequest request)
        {
            var carta = await _service.FirmarAtdtAsync(id, request);
            if (carta == null) return NotFound();
            return Ok(carta);
        }

        /// <summary>Descarga la carta responsiva como PDF.</summary>
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> DownloadPdf(long id)
        {
            var pdf = await _service.GenerarPdfAsync(id);
            if (pdf == null) return NotFound();
            return File(pdf, "application/pdf", $"Carta-{id}.pdf");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private long? ObtenerUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;
            return long.TryParse(claim, out var id) ? id : null;
        }

        private object? Validar(CartaRequestDTO dto)
        {
            if (dto.AreaRequirente == null)  return Error("areaRequirente",  "El objeto areaRequirente es requerido.");
            if (dto.AdminServidor == null)   return Error("adminServidor",   "El objeto adminServidor es requerido.");
            if (dto.Descripcion == null)     return Error("descripcion",     "El objeto descripcion es requerido.");
            if (dto.Specs == null)           return Error("specs",           "El objeto specs es requerido.");
            if (dto.Infraestructura == null) return Error("infraestructura", "El objeto infraestructura es requerido.");
            if (dto.Responsiva == null)      return Error("responsiva",      "El objeto responsiva es requerido.");

            var ar = dto.AreaRequirente;
            if (string.IsNullOrWhiteSpace(ar.Sector))      return Error("areaRequirente.sector",      "El campo sector es requerido.");
            if (string.IsNullOrWhiteSpace(ar.Dependencia)) return Error("areaRequirente.dependencia", "El campo dependencia es requerido.");
            if (string.IsNullOrWhiteSpace(ar.Responsable)) return Error("areaRequirente.responsable", "El campo responsable es requerido.");
            if (string.IsNullOrWhiteSpace(ar.Cargo))       return Error("areaRequirente.cargo",       "El campo cargo es requerido.");
            if (string.IsNullOrWhiteSpace(ar.Telefono))    return Error("areaRequirente.telefono",    "El campo telefono es requerido.");
            if (!TelefonoRegex.IsMatch(ar.Telefono!))      return Error("areaRequirente.telefono",    "Formato inválido. Usar (DDD) DDD-DDDD.");
            if (string.IsNullOrWhiteSpace(ar.Correo))      return Error("areaRequirente.correo",      "El campo correo es requerido.");
            if (!EmailRegex.IsMatch(ar.Correo!))           return Error("areaRequirente.correo",      "Correo con formato inválido.");

            var as_ = dto.AdminServidor;
            if (string.IsNullOrWhiteSpace(as_.Responsable)) return Error("adminServidor.responsable", "El campo responsable es requerido.");
            if (string.IsNullOrWhiteSpace(as_.Cargo))       return Error("adminServidor.cargo",       "El campo cargo es requerido.");
            if (!string.IsNullOrWhiteSpace(as_.Telefono) && !TelefonoRegex.IsMatch(as_.Telefono!))
                return Error("adminServidor.telefono", "Formato inválido. Usar (DDD) DDD-DDDD.");
            if (!string.IsNullOrWhiteSpace(as_.Correo) && !EmailRegex.IsMatch(as_.Correo!))
                return Error("adminServidor.correo", "Correo con formato inválido.");

            var desc = dto.Descripcion;
            if (string.IsNullOrWhiteSpace(desc.DescripcionServidor)) return Error("descripcion.descripcionServidor", "El campo descripcionServidor es requerido.");
            if (string.IsNullOrWhiteSpace(desc.NombreServidor))      return Error("descripcion.nombreServidor",      "El campo nombreServidor es requerido.");
            if (string.IsNullOrWhiteSpace(desc.FechaArranque))       return Error("descripcion.fechaArranque",       "El campo fechaArranque es requerido.");
            if (string.IsNullOrWhiteSpace(desc.TipoUso) || !TiposUso.Contains(desc.TipoUso!))
                return Error("descripcion.tipoUso", "El campo tipoUso debe ser 'interno' o 'publicado'.");

            var sp = dto.Specs;
            if (string.IsNullOrWhiteSpace(sp.TipoRequerimiento) || !TiposRequerimiento.Contains(sp.TipoRequerimiento!))
                return Error("specs.tipoRequerimiento", "El campo tipoRequerimiento debe ser 'estandar', 'especifico' o 'especial'.");
            if (string.IsNullOrWhiteSpace(sp.Modalidad) || !Modalidades.Contains(sp.Modalidad!))
                return Error("specs.modalidad", "Modalidad debe ser 'nuevo', 'renovacion', 'clonacion' o 'serverBase'.");
            if (string.IsNullOrWhiteSpace(sp.SistemaOperativo) || !SistemasOperativos.Contains(sp.SistemaOperativo!))
                return Error("specs.sistemaOperativo", "sistemaOperativo debe ser 'windows', 'linux' o 'otro'.");
            if (sp.VCores < 1)     return Error("specs.vCores",     "vCores debe ser >= 1.");
            if (sp.MemoriaRam < 1) return Error("specs.memoriaRam", "memoriaRam debe ser >= 1.");

            bool tieneDiscos = sp.DiscosDuros?.Count > 0;
            if (!tieneDiscos && sp.Almacenamiento < 1)
                return Error("specs.almacenamiento", "almacenamiento debe ser >= 1 cuando no se envían discosDuros.");
            if (tieneDiscos)
            {
                foreach (var (disco, idx) in sp.DiscosDuros!.Select((d, i) => (d, i)))
                {
                    if (disco.Capacidad < 1)
                        return Error($"specs.discosDuros[{idx}].capacidad", "Capacidad del disco debe ser >= 1.");
                    if (!string.IsNullOrWhiteSpace(disco.Tipo) && !TiposDisco.Contains(disco.Tipo!))
                        return Error($"specs.discosDuros[{idx}].tipo", "Tipo de disco debe ser 'SSD', 'HDD' o 'NVMe'.");
                }
            }

            if (string.Equals(sp.Modalidad, "renovacion", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(sp.IpActual))
                    return Error("specs.ipActual", "ipActual es requerido para modalidad 'renovacion'.");
                if (string.IsNullOrWhiteSpace(sp.NombreServidorActual))
                    return Error("specs.nombreServidorActual", "nombreServidorActual es requerido para modalidad 'renovacion'.");
            }

            var resp = dto.Responsiva;
            if (!resp.AceptaTerminos)                            return Error("responsiva.aceptaTerminos", "Se deben aceptar los términos.");
            if (string.IsNullOrWhiteSpace(resp.Firmante))       return Error("responsiva.firmante",       "El campo firmante es requerido.");
            if (string.IsNullOrWhiteSpace(resp.NumEmpleado))    return Error("responsiva.numEmpleado",    "El campo numEmpleado es requerido.");
            if (string.IsNullOrWhiteSpace(resp.PuestoFirmante)) return Error("responsiva.puestoFirmante", "El campo puestoFirmante es requerido.");

            return null;
        }

        private static object Error(string campo, string message) =>
            new { error = "VALIDATION_FAILED", message, campo };
    }
}
