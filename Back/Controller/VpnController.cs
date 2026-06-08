using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolicitudServidores.DTOs;
using SolicitudServidores.Services.Interfaces;
using System.Security.Claims;

namespace SolicitudServidores.Controllers
{
    [ApiController]
    [Route("api/vpn")]
    [Authorize]
    public class VpnController : ControllerBase
    {
        private readonly IVpnService _service;

        public VpnController(IVpnService service)
        {
            _service = service;
        }

        /// <summary>
        /// HU-18 — Busca VPNs. Admins generales ven todo; otros usuarios solo sus propias VPNs.
        /// Parámetro opcional ?folio= aplica búsqueda parcial sobre el folio de la VPN.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? folio)
        {
            var esAdmin = User.IsInRole("Administrador General");
            var claim   = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("id")?.Value;

            if (!esAdmin && !long.TryParse(claim, out _))
                return Unauthorized();

            long? userId = esAdmin ? null : long.Parse(claim!);

            return Ok(await _service.BuscarAsync(folio, userId));
        }

        /// <summary>Devuelve una VPN por su ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vpn = await _service.GetByIdAsync(id);
            if (vpn == null) return NotFound();
            return Ok(vpn);
        }

        /// <summary>Lista las VPNs asignadas a un servidor específico.</summary>
        [HttpGet("servidor/{serverId}")]
        public async Task<IActionResult> GetByServidor(long serverId)
            => Ok(await _service.GetByServidorAsync(serverId));

        /// <summary>Guarda el folio en el registro de una VPN.</summary>
        [HttpPatch("{id}/folio")]
        public async Task<IActionResult> ActualizarFolio(int id, [FromBody] ActualizarFolioVpnRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Folio))
                return BadRequest("El folio no puede estar vacío.");

            var vpn = await _service.ActualizarFolioAsync(id, request.Folio);
            if (vpn == null) return NotFound();

            return Ok(vpn);
        }

        /// <summary>Actualiza el estado de una VPN.</summary>
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Administrador de Infraestructura,Administrador General")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoVpnRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Estado))
                return BadRequest("El estado no puede estar vacío.");

            try
            {
                var vpn = await _service.UpdateEstadoAsync(id, request.Estado);
                if (vpn == null) return NotFound();
                return Ok(vpn);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>RF08 — Crea una VPN. Solo admins de infraestructura o centro de datos.</summary>
        [HttpPost]
        [Authorize(Roles = "Administrador de Infraestructura,Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> Create([FromBody] CreateVpnRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Responsable))
                return BadRequest("El campo 'responsable' es requerido.");

            try
            {
                var creada = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = creada.VpnId }, creada);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Actualiza los datos de una VPN.</summary>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Administrador de Infraestructura,Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVpnRequest request)
        {
            try
            {
                var actualizada = await _service.UpdateAsync(id, request);
                if (actualizada == null) return NotFound();
                return Ok(actualizada);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Elimina una VPN y sus asignaciones a servidores.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador General")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminada = await _service.DeleteAsync(id);
            if (eliminada == null) return NotFound();
            return Ok(eliminada);
        }

        /// <summary>RF08 — Asigna una VPN existente a un servidor.</summary>
        [HttpPost("{vpnId}/servidor/{serverId}")]
        [Authorize(Roles = "Administrador de Infraestructura,Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> AsignarAServidor(int vpnId, long serverId)
        {
            try
            {
                var asignacion = await _service.AsignarAServidorAsync(vpnId, serverId);
                return Ok(asignacion);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>Desasigna una VPN de un servidor.</summary>
        [HttpDelete("{vpnId}/servidor/{serverId}")]
        [Authorize(Roles = "Administrador de Infraestructura,Administrador de Centro de Datos,Administrador General")]
        public async Task<IActionResult> DesasignarDeServidor(int vpnId, long serverId)
        {
            var ok = await _service.DesasignarDeServidorAsync(vpnId, serverId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
