using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SolicitudServidores.Services.Interfaces;

namespace SolicitudServidores.Services.Implementaciones
{
    public class EmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _user;
        private readonly string _password;
        private readonly string _from;
        private readonly string _fromName;

        public EmailService(IConfiguration config)
        {
            _host     = config["SMTP:Host"]     ?? throw new Exception("SMTP_HOST no configurado.");
            _port     = int.Parse(config["SMTP:Port"] ?? "587");
            _user     = config["SMTP:User"]     ?? throw new Exception("SMTP_USER no configurado.");
            _password = config["SMTP:Password"] ?? throw new Exception("SMTP_PASSWORD no configurado.");
            _from     = config["SMTP:From"]     ?? _user;
            _fromName = config["SMTP:FromName"] ?? "Sistema ATDT";
        }

        public async Task SendCredencialesAsync(string destinatario, string nombreCompleto, string password)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _from));
            message.To.Add(MailboxAddress.Parse(destinatario));
            message.Subject = "Acceso al Sistema ATDT — Credenciales de ingreso";

            var body = new BodyBuilder
            {
                HtmlBody = $"""
                    <div style="font-family:Arial,sans-serif;max-width:520px;margin:auto;border:1px solid #ddd;border-radius:8px;overflow:hidden;">
                      <div style="background:#1a3a5c;padding:24px;text-align:center;">
                        <h2 style="color:#fff;margin:0;">Sistema ATDT</h2>
                        <p style="color:#a8c8e8;margin:4px 0 0;">Administración de Infraestructura</p>
                      </div>
                      <div style="padding:28px 32px;">
                        <p style="font-size:15px;">Hola <strong>{nombreCompleto}</strong>,</p>
                        <p style="font-size:15px;">Se ha creado tu cuenta en el Sistema ATDT. A continuación tus credenciales de acceso:</p>
                        <table style="width:100%;border-collapse:collapse;margin:20px 0;">
                          <tr>
                            <td style="padding:10px 14px;background:#f4f6f9;border-radius:4px 0 0 4px;font-weight:bold;color:#555;width:40%;">Usuario (correo)</td>
                            <td style="padding:10px 14px;background:#f4f6f9;border-radius:0 4px 4px 0;">{destinatario}</td>
                          </tr>
                          <tr><td colspan="2" style="padding:4px;"></td></tr>
                          <tr>
                            <td style="padding:10px 14px;background:#f4f6f9;border-radius:4px 0 0 4px;font-weight:bold;color:#555;">Contraseña temporal</td>
                            <td style="padding:10px 14px;background:#f4f6f9;border-radius:0 4px 4px 0;font-family:monospace;font-size:16px;letter-spacing:2px;">{password}</td>
                          </tr>
                        </table>
                        <div style="background:#fff8e1;border-left:4px solid #f59e0b;padding:12px 16px;border-radius:0 4px 4px 0;margin-bottom:20px;">
                          <strong>Importante:</strong> Al ingresar por primera vez, el sistema te pedirá cambiar tu contraseña.
                        </div>
                        <p style="font-size:13px;color:#888;">Si no solicitaste este acceso, ignora este correo o contacta al administrador.</p>
                      </div>
                      <div style="background:#f4f6f9;padding:14px;text-align:center;font-size:12px;color:#aaa;">
                        Sistema ATDT — Centro de Datos
                      </div>
                    </div>
                    """
            };

            message.Body = body.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_user, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
