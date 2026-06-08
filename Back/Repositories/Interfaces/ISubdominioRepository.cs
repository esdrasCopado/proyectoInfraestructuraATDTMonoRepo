using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface ISubdominioRepository
    {
        Task<Subdominio?> GetById(int id);
        Task<Subdominio?> UpdateStatus(int id, string status);
        Task<List<Subdominio>> UpdateStatusBatch(IEnumerable<int> ids, string status);
    }
}
