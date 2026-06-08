using SolicitudServidores.DTOs;
using SolicitudServidores.Helpers;
using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario> Create(Usuario usuario);
        Task<Usuario?> Update(Usuario usuario);
        Task<Usuario?> Delete(long id);
        Task<Usuario?> GetById(long id);
        Task<Usuario?> GetByEmail(string email);
        Task<Usuario?> ChangePassword(long id, string passwordHash, bool resetMustChange = false);
        string GenerateNewPassword();
        Task<List<Usuario>> GetAll(QueryUserPaging queryUser);
        Task<List<Usuario>> GetAll();
        Task<List<Usuario>> GetByRol(string rolNombre);
        Task<bool> ExistsUsuario(string email);
        Task<IEnumerable<RolDTO>> GetRoles();
    }
}
