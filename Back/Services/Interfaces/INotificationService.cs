using SolicitudServidores.DTOs;
using SolicitudServidores.Models;

namespace SolicitudServidores.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDTO>> GetByRecipientAsync(long recipientUserId, bool? leida = null);
        Task<IEnumerable<NotificationDTO>> GetByRolAsync(string rolNombre, bool? leida = null);
        Task<long> CountUnreadByRecipientAsync(long recipientUserId);
        Task<NotificationDTO?> GetByIdAsync(long id);
        Task<NotificationDTO?> MarkAsReadAsync(long id, long recipientUserId, bool leida);
        Task<NotificationDTO> CreateAsync(CreateNotificationRequest request);
        Task NotificarUsuarioAsync(long recipientUserId, string tipo, string titulo, string mensaje, long? solicitudId = null, long? senderUserId = null);
        Task NotificarPorRolAsync(string rolNombre, string tipo, string titulo, string mensaje, long? solicitudId = null, long? senderUserId = null);
        /// <summary>
        /// Envía una notificación al usuario (Dependencia / Cliente) que creó la solicitud.
        /// Lanza KeyNotFoundException si la solicitud no existe.
        /// </summary>
        Task NotificarClienteDeSolicitudAsync(long solicitudId, string tipo, string titulo, string mensaje, long? senderUserId = null);
    }
}
