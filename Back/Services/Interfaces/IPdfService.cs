using SolicitudServidores.Models;

namespace SolicitudServidores.Services.Interfaces
{
    public interface IPdfService
    {
        byte[] GenerarSolicitudPdf(Solicitud solicitud);
    }
}
