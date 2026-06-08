using Microsoft.EntityFrameworkCore;
using SolicitudServidores.DBContext;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;

namespace SolicitudServidores.Repositories.Implementaciones
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly DataContext _context;

        public NotificationRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetByRecipient(long recipientUserId, bool? leida = null)
        {
            var query = _context.Notifications
                .Where(n => n.RecipientUserId == recipientUserId)
                .OrderByDescending(n => n.CreatedAt)
                .AsQueryable();

            if (leida.HasValue)
                query = query.Where(n => n.Leida == leida.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByRol(string rolNombre, bool? leida = null)
        {
            var query = _context.Notifications
                .Include(n => n.Destinatario!)
                    .ThenInclude(u => u.Rol)
                .Where(n => n.Destinatario != null && n.Destinatario.Rol != null && n.Destinatario.Rol.Nombre == rolNombre)
                .OrderByDescending(n => n.CreatedAt)
                .AsQueryable();

            if (leida.HasValue)
                query = query.Where(n => n.Leida == leida.Value);

            return await query.ToListAsync();
        }

        public Task<long> CountUnreadByRecipient(long recipientUserId)
            => _context.Notifications.LongCountAsync(n => n.RecipientUserId == recipientUserId && !n.Leida);

        public async Task<Notification?> GetById(long id)
            => await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id);

        public async Task<Notification> Create(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            return await GetById(notification.NotificationId) ?? notification;
        }

        public async Task<Notification?> MarkAsRead(long id, long recipientUserId, bool leida)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.RecipientUserId == recipientUserId);

            if (notification == null)
                return null;

            notification.Leida = leida;
            notification.LeidaAt = leida ? DateTime.UtcNow : null;
            await _context.SaveChangesAsync();
            return await GetById(notification.NotificationId);
        }
    }
}
