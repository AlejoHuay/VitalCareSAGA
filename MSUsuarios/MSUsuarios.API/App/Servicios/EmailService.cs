using System.Net;
using System.Net.Mail;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.App.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IConfiguration configuration)
        {
            _smtpSettings = new SmtpSettings
            {
                Host = Environment.GetEnvironmentVariable("SMTP_HOST") 
                    ?? configuration.GetSection("SmtpSettings:Host").Value 
                    ?? "smtp.example.com",
                Port = int.TryParse(
                    Environment.GetEnvironmentVariable("SMTP_PORT") 
                    ?? configuration.GetSection("SmtpSettings:Port").Value, 
                    out int port) ? port : 587,
                RemitenteNombre = Environment.GetEnvironmentVariable("SMTP_REMITENTE_NOMBRE")
                    ?? configuration.GetSection("SmtpSettings:RemitenteNombre").Value
                    ?? "VitalCare",
                RemitenteEmail = Environment.GetEnvironmentVariable("SMTP_REMITENTE_EMAIL")
                    ?? configuration.GetSection("SmtpSettings:RemitenteEmail").Value
                    ?? "no-reply@example.com",
                Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                    ?? configuration.GetSection("SmtpSettings:Password").Value
                    ?? string.Empty,
                UseSsl = bool.TryParse(
                    Environment.GetEnvironmentVariable("SMTP_USE_SSL")
                    ?? configuration.GetSection("SmtpSettings:UseSsl").Value,
                    out bool ssl) ? ssl : true
            };
        }

        public Result EnviarCorreoActivacionCuenta(
            string emailDestino,
            string nombres,
            string userName,
            string passwordTemporal)
        {
            emailDestino = emailDestino?.Trim() ?? string.Empty;
            nombres = nombres?.Trim() ?? string.Empty;
            userName = userName?.Trim() ?? string.Empty;
            passwordTemporal = passwordTemporal?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(emailDestino))
                return Result.Fail("El correo destino es obligatorio.");

            if (string.IsNullOrWhiteSpace(userName))
                return Result.Fail("El nombre de usuario es obligatorio para el correo.");

            if (string.IsNullOrWhiteSpace(passwordTemporal))
                return Result.Fail("La contraseña temporal es obligatoria para el correo.");

            try
            {
                string asunto = "Activación de cuenta - Farmacia VitalCare";
                string cuerpoHtml = ConstruirHtmlActivacionCuenta(
                    nombres,
                    userName,
                    passwordTemporal
                );

                using MailMessage message = new MailMessage();
                message.From = new MailAddress(
                    _smtpSettings.RemitenteEmail,
                    _smtpSettings.RemitenteNombre
                );
                message.To.Add(emailDestino);
                message.Subject = asunto;
                message.Body = cuerpoHtml;
                message.IsBodyHtml = true;

                using SmtpClient client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port);
                client.Credentials = new NetworkCredential(
                    _smtpSettings.RemitenteEmail,
                    _smtpSettings.Password
                );
                client.EnableSsl = _smtpSettings.UseSsl;
                client.Timeout = 10000; // 10 segundos timeout

                client.Send(message);

                return Result.Ok();
            }
            catch (SmtpException smtpEx)
            {
                return Result.Fail($"Error SMTP: No se pudo conectar con el servidor de correo. {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                return Result.Fail($"No se pudo enviar el correo electronico. Detalle: {ex.Message}");
            }
        }



        private string ConstruirHtmlActivacionCuenta(
            string nombres,
            string userName,
            string passwordTemporal)
        {
            string saludo = string.IsNullOrWhiteSpace(nombres) ? "Estimado usuario" : $"Estimado/a {nombres}";

            return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
        </head>
        <body style='margin:0; padding:0; background-color:#f0f7f5; font-family:Arial, sans-serif; color:#1f2937;'>

        <table width='100%' cellpadding='0' cellspacing='0' style='padding:30px 0;'>
        <tr>
        <td align='center'>

        <table width='600' style='background:#ffffff; border-radius:14px; overflow:hidden; box-shadow:0 4px 18px rgba(0,0,0,0.08);'>

        <tr>
        <td style='background:linear-gradient(135deg, #1f7a63, #14532d); padding:30px; text-align:center;'>
            <h1 style='margin:0; color:white;'>Activación de cuenta</h1>
            <p style='margin:8px 0 0; color:#d1fae5;'>Farmacia VitalCare</p>
        </td>
        </tr>

        <tr>
        <td style='padding:40px;'>

        <p style='font-size:16px;'>{saludo},</p>

        <p style='color:#4b5563; line-height:1.6;'>
        Tu cuenta ha sido registrada correctamente. Estas son tus credenciales:
        </p>

        <table width='100%' style='margin:20px 0; background:#ecfdf5; border:1px solid #bbf7d0; border-radius:10px;'>
        <tr>
        <td style='padding:20px;'>

        <p><strong>Usuario:</strong>
        <span style='color:#065f46;'>{userName}</span></p>

        <p><strong>Contraseña:</strong>
        <span style='color:#b91c1c; font-weight:bold;'>{passwordTemporal}</span></p>

        </td>
        </tr>
        </table>

        <p style='color:#4b5563;'>
        Utiliza estas credenciales para iniciar sesión en el sistema. Por tu seguridad, te recomendamos cambiar tu contraseña al primer acceso.
        </p>

        <div style='text-align:center; margin:30px 0;'>
            <a href='http://localhost:5081/Auth/Login' style='display:inline-block; background-color:#1f7a63; color:white; padding:12px 30px; text-decoration:none; border-radius:6px; font-weight:bold; font-size:16px;'>
                Ir al Login
            </a>
        </div>

        <p>Atentamente,<br><strong>Farmacia VitalCare</strong></p>

        </td>
        </tr>

        <tr>
        <td style='padding:20px; text-align:center; background:#ecfdf5; font-size:12px; color:#6b7280;'>
        Mensaje automático - no responder
        </td>
        </tr>

        </table>

        </td>
        </tr>
        </table>

        </body>
        </html>";
        }


    }
}
