using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/dependencia")]
    public class DependencyController : ControllerBase
    {
        private readonly IDependencyRepository _repo;

        public DependencyController(IDependencyRepository repo)
        {
            _repo = repo;
        }

        /// <summary>Devuelve todas las dependencias registradas.</summary>
        /// <remarks>Usado al crear usuarios con rol "Dependencia / Cliente" para seleccionar a qué dependencia pertenece.</remarks>
        /// <response code="200">Lista de dependencias ordenada por nombre.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DependencyDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var deps = await _repo.GetAll();
            return Ok(deps.Select(MapToDto));
        }

        /// <summary>Devuelve una dependencia por su ID.</summary>
        /// <response code="200">Dependencia encontrada.</response>
        /// <response code="404">Dependencia no encontrada.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DependencyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var dep = await _repo.GetById(id);
            if (dep == null) return NotFound();
            return Ok(MapToDto(dep));
        }

        /// <summary>Crea una nueva dependencia.</summary>
        /// <remarks>
        /// Solo se requiere <c>name</c>. Los demás campos son opcionales.
        /// Si ya existe una dependencia con el mismo nombre se retorna 409.
        /// </remarks>
        /// <response code="201">Dependencia creada.</response>
        /// <response code="400">Nombre no proporcionado.</response>
        /// <response code="409">Ya existe una dependencia con ese nombre.</response>
        [HttpPost]
        [ProducesResponseType(typeof(DependencyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateDependencyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("El nombre de la dependencia es requerido.");

            if (await _repo.Exists(request.Name.Trim()))
                return Conflict("Ya existe una dependencia con ese nombre.");

            var dependency = new Dependency
            {
                Name        = request.Name.Trim(),
                Sector      = request.Sector?.Trim(),
                Responsable = request.Responsable?.Trim(),
                Cargo       = request.Cargo?.Trim(),
                Phone       = request.Phone?.Trim(),
                Email       = request.Email?.Trim()
            };

            var creada = await _repo.Create(dependency);
            return CreatedAtAction(nameof(GetById), new { id = creada.DependencyId }, MapToDto(creada));
        }

        private static DependencyDTO MapToDto(Dependency d) => new()
        {
            DependencyId = d.DependencyId,
            Name         = d.Name,
            Sector       = d.Sector,
            Responsable  = d.Responsable,
            Cargo        = d.Cargo,
            Phone        = d.Phone,
            Email        = d.Email
        };
    }
}
