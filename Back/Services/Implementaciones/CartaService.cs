using System.Text;
using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Services.Implementaciones
{
    public class CartaService : ICartaService
    {
        private readonly ICartaRepository _repo;
        private readonly ISeguimientoService _seguimiento;
        private readonly DataContext _context;

        public CartaService(
            ICartaRepository repo,
            ISeguimientoService seguimiento,
            DataContext context)
        {
            _repo        = repo;
            _seguimiento = seguimiento;
            _context     = context;
        }

        public Task<IEnumerable<Carta>> GetAllAsync() => _repo.GetAll();

        public Task<Carta?> GetByIdAsync(long id) => _repo.GetById(id);

        public Task<Carta?> GetBySolicitudAsync(long solicitudId) => _repo.GetBySolicitudId(solicitudId);

        // ── RF02: Crea Dependency + AdminContact + Solicitud + Carta + Seguimiento ──
        public async Task<Carta> CreateAsync(CartaRequestDTO dto, long createdBy)
        {
            var ar   = dto.AreaRequirente!;
            var as_  = dto.AdminServidor!;
            var desc = dto.Descripcion!;
            var sp   = dto.Specs!;
            var resp = dto.Responsiva!;

            // 1. Dependency
            var dependency = new Dependency
            {
                Sector      = ar.Sector,
                Name        = ar.Dependencia!,
                Responsable = ar.Responsable,
                Cargo       = ar.Cargo,
                Phone       = ar.Telefono,
                Email       = ar.Correo,
            };
            await _context.Dependencies.AddAsync(dependency);
            await _context.SaveChangesAsync();

            // 2. AdminDepContactInformation
            var adminContact = new AdminDepContactInformation
            {
                DependencyId  = dependency.DependencyId,
                Proveedor     = as_.Proveedor,
                AdminServidor = as_.Responsable!,
                Cargo         = as_.Cargo,
                Phone         = as_.Telefono,
                Email         = as_.Correo,
            };
            await _context.AdminDepContacts.AddAsync(adminContact);
            await _context.SaveChangesAsync();

            // 3. Solicitud
            int almacenamiento = sp.DiscosDuros?.Sum(d => d.Capacidad) is int suma && suma > 0
                ? suma
                : sp.Almacenamiento;

            var folio = await GenerarFolioSolicitudAsync();

            var solicitud = new Solicitud
            {
                Folio                    = folio,
                DependencyId             = dependency.DependencyId,
                AdminContactId           = adminContact.AdminContactId,
                DescripcionUso           = desc.DescripcionServidor!,
                NombreServidor           = desc.NombreServidor!,
                NombreAplicacion         = desc.NombreAplicacion,
                TipoUso                  = desc.TipoUso!,
                FechaArranqueDeseada     = DateTime.TryParse(desc.FechaArranque, out var fa) ? fa.Date : null,
                VigenciaMeses            = int.TryParse(desc.Vigencia, out var v) ? v : 12,
                CaracteristicasEspeciales = desc.CaracteristicasEspeciales,
                TipoRequerimiento        = sp.TipoRequerimiento!,
                EsClonacion              = string.Equals(sp.Modalidad, "clonacion", StringComparison.OrdinalIgnoreCase),
                IpServidorBase           = sp.IpActual,
                NombreServidorBase       = sp.NombreServidorActual,
                SistemaOperativo         = sp.SistemaOperativoOtro ?? sp.SistemaOperativo,
                RamSolicitadaGb          = sp.MemoriaRam,
                VcpuSolicitado           = sp.VCores,
                AlmacenamientoSolicitadoGb = almacenamiento,
                MotorBaseDatos           = sp.MotorBD,
                ReglasFirewall           = sp.Puertos,
                IntegracionesExternas    = sp.Integraciones,
                ConectividadOtras        = sp.OtrasSpecs,
                Estatus                  = "pendiente",
                CreatedBy                = createdBy,
                CreatedAt                = DateTime.UtcNow,
                UpdatedAt                = DateTime.UtcNow,
            };

            await _context.Solicitudes.AddAsync(solicitud);
            await _context.SaveChangesAsync();

            // 4. Carta
            var folioCarta = await GenerarFolioCartaAsync();

            var carta = new Carta
            {
                SolicitudId                  = solicitud.Id,
                FolioCarta                   = folioCarta,
                FirmanteDependenciaNombre    = resp.Firmante,
                FirmanteDependenciaPuesto    = resp.PuestoFirmante,
                FirmanteDependenciaEmpleado  = resp.NumEmpleado,
                CreatedBy                    = createdBy,
                CreatedAt                    = DateTime.UtcNow,
                UpdatedAt                    = DateTime.UtcNow,
            };

            var cartaCreada = await _repo.Create(carta);

            // 5. Seguimiento — inicializar las 14 etapas y marcar la primera en proceso
            await _seguimiento.InicializarEtapasAsync(solicitud.Id);
            await _seguimiento.AvanzarEtapaAsync(solicitud.Id, 1, new AvanzarEtapaRequest
            {
                Status       = "en_proceso",
                CompletadoBy = createdBy,
                Observaciones = $"Carta responsiva registrada con folio {folioCarta}."
            });

            return cartaCreada;
        }

        // ── RF02: Firma de la Dependencia ─────────────────────────────────────────
        public async Task<Carta?> FirmarDependenciaAsync(long id, FirmaDependenciaRequest request)
        {
            var carta = await _repo.GetById(id);
            if (carta == null) return null;

            carta.FirmanteDependenciaNombre   = request.FirmanteNombre;
            carta.FirmanteDependenciaPuesto   = request.FirmantePuesto;
            carta.FirmanteDependenciaEmpleado = request.FirmanteNumEmpleado;
            carta.FirmaDependenciaAt          = DateTime.UtcNow;

            return await _repo.Update(carta);
        }

        // ── RF02: Firma del ATDT ──────────────────────────────────────────────────
        public async Task<Carta?> FirmarAtdtAsync(long id, FirmaAtdtRequest request)
        {
            var carta = await _repo.GetById(id);
            if (carta == null) return null;

            if (!string.IsNullOrWhiteSpace(request.FirmanteNombre))
                carta.FirmanteAtdtNombre = request.FirmanteNombre;

            if (!string.IsNullOrWhiteSpace(request.FirmantePuesto))
                carta.FirmanteAtdtPuesto = request.FirmantePuesto;

            carta.FirmaAtdtAt = DateTime.UtcNow;

            // Cuando el ATDT firma, la etapa carta_responsiva se completa
            if (carta.SolicitudId > 0)
            {
                await _seguimiento.AvanzarEtapaAsync(carta.SolicitudId, 1, new AvanzarEtapaRequest
                {
                    Status        = "completado",
                    Observaciones = "Carta responsiva firmada por ATDT."
                });
            }

            return await _repo.Update(carta);
        }

        // ── PDF ───────────────────────────────────────────────────────────────────
        public async Task<byte[]?> GenerarPdfAsync(long id)
        {
            var carta = await _repo.GetById(id);
            if (carta == null) return null;

            carta.PdfGeneratedAt = DateTime.UtcNow;
            await _repo.Update(carta);

            return BuildSimplePdf(carta);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private async Task<string> GenerarFolioCartaAsync()
        {
            var year  = DateTime.UtcNow.Year;
            var count = await _context.Cartas.CountAsync(c => c.CreatedAt.Year == year);
            return $"ATDT-{year}-{(count + 1):D4}";
        }

        private Task<string> GenerarFolioSolicitudAsync()
        {
            return Task.FromResult($"SOL-{DateTime.UtcNow:yyyyMMdd-HHmmss}");
        }

        private static byte[] BuildSimplePdf(Carta carta)
        {
            var dep = carta.Solicitud?.Dependency;

            var lines = new[]
            {
                $"Carta Responsiva de Aprovisionamiento - {carta.FolioCarta}",
                $"Solicitud folio: {carta.Solicitud?.Folio ?? "N/D"}",
                $"Dependencia: {dep?.Name ?? "N/D"}",
                $"Responsable área requirente: {dep?.Responsable ?? "N/D"}",
                $"Servidor solicitado: {carta.Solicitud?.NombreServidor ?? "N/D"}",
                $"Aplicación: {carta.Solicitud?.NombreAplicacion ?? "N/D"}",
                $"Tipo de uso: {carta.Solicitud?.TipoUso ?? "N/D"}",
                $"Sistema operativo: {carta.Solicitud?.SistemaOperativo ?? "N/D"}",
                $"vCPU: {carta.Solicitud?.VcpuSolicitado}  RAM: {carta.Solicitud?.RamSolicitadaGb} GB  Almacenamiento: {carta.Solicitud?.AlmacenamientoSolicitadoGb} GB",
                $"Firmante dependencia: {carta.FirmanteDependenciaNombre ?? "Pendiente"} ({carta.FirmanteDependenciaPuesto})",
                $"N° empleado: {carta.FirmanteDependenciaEmpleado ?? "N/D"}",
                $"Firma dependencia: {(carta.FirmaDependenciaAt.HasValue ? carta.FirmaDependenciaAt.Value.ToString("dd/MM/yyyy") : "Pendiente")}",
                $"Firmante ATDT: {carta.FirmanteAtdtNombre} ({carta.FirmanteAtdtPuesto})",
                $"Firma ATDT: {(carta.FirmaAtdtAt.HasValue ? carta.FirmaAtdtAt.Value.ToString("dd/MM/yyyy") : "Pendiente")}",
            };

            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine("BT");
            contentBuilder.AppendLine("/F1 11 Tf");
            contentBuilder.AppendLine("72 760 Td");
            foreach (var line in lines)
            {
                contentBuilder.AppendLine($"({EscapePdfText(line)}) Tj");
                contentBuilder.AppendLine("0 -18 Td");
            }
            contentBuilder.AppendLine("ET");

            var stream  = contentBuilder.ToString();
            var objects = new List<string>
            {
                "1 0 obj << /Type /Catalog /Pages 2 0 R >> endobj",
                "2 0 obj << /Type /Pages /Count 1 /Kids [3 0 R] >> endobj",
                "3 0 obj << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >> endobj",
                $"4 0 obj << /Length {Encoding.ASCII.GetByteCount(stream)} >> stream\n{stream}endstream endobj",
                "5 0 obj << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> endobj"
            };

            var pdf = new StringBuilder();
            pdf.AppendLine("%PDF-1.4");
            var offsets = new List<int>();
            foreach (var obj in objects)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.AppendLine(obj);
            }

            var xrefPos = Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.AppendLine($"xref\n0 {objects.Count + 1}");
            pdf.AppendLine("0000000000 65535 f ");
            foreach (var off in offsets) pdf.AppendLine($"{off:D10} 00000 n ");
            pdf.AppendLine($"trailer << /Size {objects.Count + 1} /Root 1 0 R >>");
            pdf.AppendLine("startxref");
            pdf.AppendLine(xrefPos.ToString());
            pdf.Append("%%EOF");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }

        private static string EscapePdfText(string text) =>
            text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
