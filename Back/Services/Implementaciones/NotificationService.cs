using SolicitudServidores.DTOs;
using SolicitudServidores.Models;
using SolicitudServidores.Repositories.Interfaces;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Services.Implementaciones
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ISolicitudRepository _solicitudRepo;

        public NotificationService(INotificationRepository repo, IUsuarioRepository usuarioRepo, ISolicitudRepository solicitudRepo)
        {
            _repo          = repo;
            _usuarioRepo   = usuarioRepo;
            _solicitudRepo = solicitudRepo;
        }

        public async Task<IEnumerable<NotificationDTO>> GetByRecipientAsync(long recipientUserId, bool? leida = null)
        {
            var notifications = await _repo.GetByRecipient(recipientUserId, leida);
            return notifications.Select(MapToDto);
        }

        public async Task<IEnumerable<NotificationDTO>> GetByRolAsync(string rolNombre, bool? leida = null)
        {
            var notifications = await _repo.GetByRol(rolNombre, leida);
            return notifications.Select(MapToDto);
        }

        public Task<long> CountUnreadByRecipientAsync(long recipientUserId)
            => _repo.CountUnreadByRecipient(recipientUserId);

        public async Task<NotificationDTO?> GetByIdAsync(long id)
        {
            var notification = await _repo.GetById(id);
            return notification == null ? null : MapToDto(notification);
        }

        public async Task<NotificationDTO?> MarkAsReadAsync(long id, long recipientUserId, bool leida)
        {
            var notification = await _repo.MarkAsRead(id, recipientUserId, leida);
            return notification == null ? null : MapToDto(notification);
        }

        public async Task<NotificationDTO> CreateAsync(CreateNotificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Tipo))
                throw new ArgumentException("El campo 'tipo' es requerido.");

            if (string.IsNullOrWhiteSpace(request.Titulo))
                throw new ArgumentException("El campo 'titulo' es requerido.");

            if (string.IsNullOrWhiteSpace(request.Mensaje))
                throw new ArgumentException("El campo 'mensaje' es requerido.");

            var notification = new Notification
            {
                RecipientUserId = request.RecipientUserId,
                SenderUserId = request.SenderUserId,
                Tipo = request.Tipo.Trim(),
                SolicitudId = request.SolicitudId,
                EntityType = request.EntityType?.Trim(),
                EntityId = request.EntityId,
                Titulo = request.Titulo.Trim(),
                Mensaje = request.Mensaje.Trim(),
                CreatedAt = DateTime.UtcNow,
            };

            return MapToDto(await _repo.Create(notification));
        }

        public async Task NotificarUsuarioAsync(long recipientUserId, string tipo, string titulo, string mensaje, long? solicitudId = null, long? senderUserId = null)
        {
            await CreateAsync(new CreateNotificationRequest
            {
                RecipientUserId = recipientUserId,
                SenderUserId    = senderUserId,
                Tipo            = tipo,
                SolicitudId     = solicitudId,
                Titulo          = titulo,
                Mensaje         = mensaje,
            });
        }

        public async Task NotificarPorRolAsync(string rolNombre, string tipo, string titulo, string mensaje, long? solicitudId = null, long? senderUserId = null)
        {
            var usuarios = await _usuarioRepo.GetByRol(rolNombre);
            foreach (var u in usuarios)
                await NotificarUsuarioAsync(u.Id, tipo, titulo, mensaje, solicitudId, senderUserId);
        }

        public async Task NotificarClienteDeSolicitudAsync(long solicitudId, string tipo, string titulo, string mensaje, long? senderUserId = null)
        {
            var solicitud = await _solicitudRepo.GetById(solicitudId)
                ?? throw new KeyNotFoundException($"No existe la solicitud con id {solicitudId}.");

            await NotificarUsuarioAsync(solicitud.CreatedBy, tipo, titulo, mensaje, solicitudId, senderUserId);
        }

        private static NotificationDTO MapToDto(Notification notification)
            => new NotificationDTO
            {
                NotificationId = notification.NotificationId,
                RecipientUserId = notification.RecipientUserId,
                SenderUserId = notification.SenderUserId,
                Tipo = notification.Tipo,
                SolicitudId = notification.SolicitudId,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                Titulo = notification.Titulo,
                Mensaje = notification.Mensaje,
                Leida = notification.Leida,
                LeidaAt = notification.LeidaAt,
                CreatedAt = notification.CreatedAt,
            };
    }
}
