using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface ICartaRepository
    {
        Task<IEnumerable<Carta>> GetAll();
        Task<Carta?> GetById(long id);
        Task<Carta?> GetBySolicitudId(long solicitudId);
        Task<Carta> Create(Carta carta);
        Task<Carta?> Update(Carta carta);
    }
}
