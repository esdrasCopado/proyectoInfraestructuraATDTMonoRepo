using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.DTOs;
using SolicitudServidores.Helpers;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DataContext _context;

        public UsuarioRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Usuario> Create(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();
            return await GetById(usuario.Id) ?? usuario;
        }

        public async Task<Usuario?> Delete(long id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (usuario == null) return null;

            usuario.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return usuario;
        }

        public Task<bool> ExistsUsuario(string email)
        {
            return _context.Usuarios.AnyAsync(x => x.Email == email && x.DeletedAt == null);
        }

        public async Task<List<Usuario>> GetAll(QueryUserPaging query)
        {
            var usuarios = _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Dependency)
                .Where(u => u.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Role))
                usuarios = usuarios.Where(u => u.Rol != null && u.Rol.Nombre == query.Role);

            var skipNumber = (query.NumPage - 1) * query.NumSize;
            return await usuarios
                .OrderBy(u => u.Apellidos).ThenBy(u => u.Nombre)
                .Skip(skipNumber)
                .Take(query.NumSize)
                .ToListAsync();
        }

        public async Task<List<Usuario>> GetAll()
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Dependency)
                .Where(u => u.DeletedAt == null)
                .OrderBy(u => u.Apellidos).ThenBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<List<Usuario>> GetByRol(string rolNombre)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.DeletedAt == null && u.Activo && u.Rol != null && u.Rol.Nombre == rolNombre)
                .ToListAsync();
        }

        public async Task<Usuario?> GetById(long id)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Dependency)
                .FirstOrDefaultAsync(u => u.Id == id && u.DeletedAt == null);
        }

        public async Task<Usuario?> GetByEmail(string email)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Dependency)
                .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);
        }

        public async Task<Usuario?> Update(Usuario usuario)
        {
            var model = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Id == usuario.Id && x.DeletedAt == null);

            if (model == null) return null;

            model.Nombre         = usuario.Nombre;
            model.Apellidos      = usuario.Apellidos;
            model.Email          = usuario.Email;
            model.RoleId         = usuario.RoleId;
            model.DependencyId   = usuario.DependencyId;
            model.NumeroEmpleado = usuario.NumeroEmpleado;
            model.Cargo          = usuario.Cargo;
            model.Phone          = usuario.Phone;
            model.Activo         = usuario.Activo;
            model.UpdatedBy      = usuario.UpdatedBy;
            model.UpdatedAt      = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(usuario.PasswordHash))
                model.PasswordHash = usuario.PasswordHash;

            await _context.SaveChangesAsync();
            return await GetById(model.Id);
        }

        public async Task<Usuario?> ChangePassword(long id, string passwordHash, bool resetMustChange = false)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
            if (usuario == null) return null;

            usuario.PasswordHash = passwordHash;
            usuario.UpdatedAt    = DateTime.UtcNow;
            if (resetMustChange)
                usuario.MustChangePassword = false;
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<IEnumerable<RolDTO>> GetRoles()
        {
            return await _context.Roles
                .OrderBy(r => r.Nombre)
                .Select(r => new RolDTO { RoleId = r.RoleId, Nombre = r.Nombre, Descripcion = r.Descripcion })
                .ToListAsync();
        }

        public string GenerateNewPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
