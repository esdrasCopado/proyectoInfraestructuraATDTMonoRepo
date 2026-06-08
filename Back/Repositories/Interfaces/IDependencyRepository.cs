using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface IDependencyRepository
    {
        Task<List<Dependency>> GetAll();
        Task<Dependency?> GetById(int id);
        Task<bool> Exists(string name);
        Task<Dependency> Create(Dependency dependency);
    }
}
