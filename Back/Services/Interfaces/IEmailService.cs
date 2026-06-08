namespace SolicitudServidores.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendCredencialesAsync(string destinatario, string nombreCompleto, string password);
    }
}
