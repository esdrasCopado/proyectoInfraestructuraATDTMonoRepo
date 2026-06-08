using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface IServidorRepository
    {
        Task<Servidor> Create(Servidor servidor);
        Task<Servidor?> Update(Servidor servidor);
        Task<Servidor?> Delete(long id);
        Task<Servidor?> GetById(long id);
        Task<List<Servidor>> GetAll();
        Task<List<Servidor>> GetBySolicitud(long idSolicitud);
        Task<bool> ExistsServidor(long id);
        Task<Servidor?> GetByIp(string ip);
        Task<List<Subdominio>> UpdateSubdominiosStatusByIp(string ip, string nuevoStatus);
    }
}