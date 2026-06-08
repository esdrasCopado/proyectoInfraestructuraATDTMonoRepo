using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class DependencyRepository : IDependencyRepository
    {
        private readonly DataContext _context;

        public DependencyRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Dependency>> GetAll()
        {
            return await _context.Dependencies
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Dependency?> GetById(int id)
        {
            return await _context.Dependencies.FindAsync(id);
        }

        public Task<bool> Exists(string name)
        {
            return _context.Dependencies.AnyAsync(d => d.Name.ToLower() == name.ToLower());
        }

        public async Task<Dependency> Create(Dependency dependency)
        {
            await _context.Dependencies.AddAsync(dependency);
            await _context.SaveChangesAsync();
            return dependency;
        }
    }
}
