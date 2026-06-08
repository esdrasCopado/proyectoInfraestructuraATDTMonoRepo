using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Services.Implementaciones
{
    public class EvidenciaService : IEvidenciaService
    {
        private readonly IEvidenciaRepository _repo;
        private readonly IWebHostEnvironment _env;

        private static readonly HashSet<string> PropositosValidos =
            new() { "pruebas_funcionamiento", "solventacion_vulnerabilidades" };

        public EvidenciaService(IEvidenciaRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env  = env;
        }

        public async Task<EvidenciaResponseDto> SubirAsync(long solicitudId, string proposito, IFormFile archivo, long userId)
        {
            if (!PropositosValidos.Contains(proposito))
                throw new ArgumentException($"Propósito inválido. Valores permitidos: {string.Join(", ", PropositosValidos)}");

            if (archivo.Length == 0)
                throw new ArgumentException("El archivo está vacío.");

            if (!archivo.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
                && !archivo.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Solo se permiten archivos PDF.");

            var ronda = await _repo.ContarPorPropositoYSolicitud(solicitudId, proposito) + 1;

            var carpeta = Path.Combine(_env.WebRootPath, "evidencias", solicitudId.ToString());
            Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{proposito}_ronda{ronda}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(archivo.FileName)}";
            var rutaCompleta  = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                await archivo.CopyToAsync(stream);

            var evidencia = new Evidencia
            {
                SolicitudId      = solicitudId,
                Proposito        = proposito,
                Ronda            = ronda,
                ArchivoNombre    = nombreArchivo,
                ArchivoPath      = Path.Combine("evidencias", solicitudId.ToString(), nombreArchivo),
                ArchivoSizeKb    = (int)Math.Ceiling(archivo.Length / 1024.0),
                UploadedBy       = userId,
                UploadedAt       = DateTime.UtcNow,
                EstadoValidacion = "pendiente",
            };

            return Mapear(await _repo.Create(evidencia));
        }

        public async Task<IEnumerable<EvidenciaResponseDto>> GetBySolicitudAsync(long solicitudId)
            => (await _repo.GetBySolicitud(solicitudId)).Select(Mapear);

        public async Task<EvidenciaResponseDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetById(id);
            return e == null ? null : Mapear(e);
        }

        public async Task<EvidenciaResponseDto?> ValidarAsync(long id, string estado, string? motivo, long validadoPor)
        {
            var estadosValidos = new[] { "aprobada", "rechazada" };
            if (!estadosValidos.Contains(estado))
                throw new ArgumentException($"Estado inválido. Valores permitidos: {string.Join(", ", estadosValidos)}");

            if (estado == "rechazada" && string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de rechazo es requerido cuando se rechaza una evidencia.");

            var e = await _repo.Validar(id, estado, motivo, validadoPor);
            return e == null ? null : Mapear(e);
        }

        public Task<Evidencia?> EliminarAsync(long id)
            => _repo.SoftDelete(id);

        public async Task<int> EliminarPorSolicitudYPropositoAsync(long solicitudId, string proposito)
        {
            var eliminadas = (await _repo.EliminarPorSolicitudYProposito(solicitudId, proposito)).ToList();

            foreach (var e in eliminadas)
            {
                var rutaFisica = Path.Combine(_env.WebRootPath, e.ArchivoPath);
                if (File.Exists(rutaFisica))
                    File.Delete(rutaFisica);
            }

            return eliminadas.Count;
        }

        public async Task<int> RechazarEvidenciasAsync(List<EvidenciaRechazadaItem> items, long rechazadoBy)
        {
            var mapped = items.Select(i => (i.Id, i.Motivo)).ToList();
            var eliminadas = (await _repo.RechazarYEliminar(mapped, rechazadoBy)).ToList();

            foreach (var e in eliminadas)
            {
                var rutaFisica = Path.Combine(_env.WebRootPath, e.ArchivoPath);
                if (File.Exists(rutaFisica))
                    File.Delete(rutaFisica);
            }

            return eliminadas.Count;
        }

        public async Task<IEnumerable<EvidenciaResponseDto>> GetAllBySolicitudAsync(long solicitudId)
            => (await _repo.GetAllBySolicitud(solicitudId)).Select(Mapear);

        private static EvidenciaResponseDto Mapear(Evidencia e) => new()
        {
            Id               = e.Id,
            SolicitudId      = e.SolicitudId,
            Proposito        = e.Proposito,
            Ronda            = e.Ronda,
            ArchivoNombre    = e.ArchivoNombre,
            ArchivoPath      = e.ArchivoPath,
            ArchivoSizeKb    = e.ArchivoSizeKb,
            EstadoValidacion = e.EstadoValidacion,
            MotivoRechazo    = e.MotivoRechazo,
            UploadedAt       = e.UploadedAt,
            ValidatedAt      = e.ValidatedAt,
            DeletedAt        = e.DeletedAt,
            SubidoPor        = e.SubidoPor == null ? null : new UsuarioResumenDto
            {
                Id            = e.SubidoPor.Id,
                NombreCompleto = $"{e.SubidoPor.Nombre} {e.SubidoPor.Apellidos}".Trim(),
                Email         = e.SubidoPor.Email,
                Cargo         = e.SubidoPor.Cargo,
            },
            ValidadoPor = e.ValidadoPor == null ? null : new UsuarioResumenDto
            {
                Id            = e.ValidadoPor.Id,
                NombreCompleto = $"{e.ValidadoPor.Nombre} {e.ValidadoPor.Apellidos}".Trim(),
                Email         = e.ValidadoPor.Email,
                Cargo         = e.ValidadoPor.Cargo,
            },
        };
    }
}
