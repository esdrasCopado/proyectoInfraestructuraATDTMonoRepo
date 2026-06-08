using SolicitudServidores.Models;

namespace SolicitudServidores.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByRecipient(long recipientUserId, bool? leida = null);
        Task<IEnumerable<Notification>> GetByRol(string rolNombre, bool? leida = null);
        Task<long> CountUnreadByRecipient(long recipientUserId);
        Task<Notification?> GetById(long id);
        Task<Notification> Create(Notification notification);
        Task<Notification?> MarkAsRead(long id, long recipientUserId, bool leida);
    }
}
