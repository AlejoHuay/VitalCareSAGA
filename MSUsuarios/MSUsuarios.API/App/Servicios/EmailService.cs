using System.Net;
using System.Net.Mail;
using ServicioUsuarios.App.Interfaces;
using ServicioUsuarios.Dominio.Modelos;
using ServicioUsuarios.Dominio.Validadores;

namespace ServicioUsuarios.App.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IConfiguration configuration)
        {
            _smtpSettings = configuration
                .GetSection("SmtpSettings")
                .Get<SmtpSettings>() ?? new SmtpSettings();
        }

        public Result EnviarCorreoActivacionCuenta(
            string emailDestino,
            string nombres,
            string userName,
            string passwordTemporal,
            string enlaceActivacion)
        {
            emailDestino = emailDestino?.Trim() ?? string.Empty;
            nombres = nombres?.Trim() ?? string.Empty;
            userName = userName?.Trim() ?? string.Empty;
            passwordTemporal = passwordTemporal?.Trim() ?? string.Empty;
            enlaceActivacion = enlaceActivacion?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(emailDestino))
                return Result.Fail("El correo destino es obligatorio.");

            if (string.IsNullOrWhiteSpace(userName))
                return Result.Fail("El nombre de usuario es obligatorio para el correo.");

            if (string.IsNullOrWhiteSpace(passwordTemporal))
                return Result.Fail("La contrasena temporal es obligatoria para el correo.");

            if (string.IsNullOrWhiteSpace(enlaceActivacion))
                return Result.Fail("El enlace de activacion es obligatorio.");

            try
            {
                string asunto = "Activacion de cuenta - Farmacia VitalCare";
                string cuerpoHtml = ConstruirHtmlActivacionCuenta(
                    nombres,
                    userName,
                    passwordTemporal,
                    enlaceActivacion
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

                client.Send(message);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"No se pudo enviar el correo electronico. Detalle: {ex.Message}");
            }
        }

        private string ConstruirHtmlActivacionCuenta(
            string nombres,
            string userName,
            string passwordTemporal,
            string enlaceActivacion)
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
            <h1 style='margin:0; color:white;'>Activacion de cuenta</h1>
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

        <p><strong>Contrasena:</strong>
        <span style='color:#b91c1c; font-weight:bold;'>{passwordTemporal}</span></p>

        </td>
        </tr>
        </table>

        <p style='color:#4b5563;'>
        Por seguridad, debes activar tu cuenta y cambiar tu contrasena:
        </p>

        <table align='center' style='margin:25px auto;'>
        <tr>
        <td style='background:#1f7a63; border-radius:8px;'>

        <a href='{enlaceActivacion}'
        style='display:inline-block; padding:14px 26px; color:white; text-decoration:none; font-weight:bold;'>
        Activar cuenta
        </a>

        </td>
        </tr>
        </table>

        <p style='font-size:13px; color:#6b7280;'>
        Si el boton no funciona:
        </p>

        <p style='font-size:12px; word-break:break-all;'>
        <a href='{enlaceActivacion}' style='color:#1f7a63;'>{enlaceActivacion}</a>
        </p>

        <p style='font-size:14px; color:#6b7280;'>
        Este enlace expirara segun la politica del sistema.
        </p>

        <p>Atentamente,<br><strong>Farmacia VitalCare</strong></p>

        </td>
        </tr>

        <tr>
        <td style='padding:20px; text-align:center; background:#ecfdf5; font-size:12px; color:#6b7280;'>
        Mensaje automatico - no responder
        </td>
        </tr>

        </table>

        </td>
        </tr>
        </table>

        </body>
        </html>";
        }

        public Result EnviarCorreoRecuperacionContrasena(
            string emailDestino,
            string nombres,
            string userName,
            string enlaceRecuperacion)
        {
            emailDestino = emailDestino?.Trim() ?? string.Empty;
            nombres = nombres?.Trim() ?? string.Empty;
            userName = userName?.Trim() ?? string.Empty;
            enlaceRecuperacion = enlaceRecuperacion?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(emailDestino))
                return Result.Fail("El correo destino es obligatorio.");

            if (string.IsNullOrWhiteSpace(userName))
                return Result.Fail("El nombre de usuario es obligatorio para el correo.");

            if (string.IsNullOrWhiteSpace(enlaceRecuperacion))
                return Result.Fail("El enlace de recuperacion es obligatorio.");

            try
            {
                string asunto = "Recuperacion de contraseña - Farmacia VitalCare";
                string cuerpoHtml = ConstruirHtmlRecuperacionContrasena(
                    nombres,
                    userName,
                    enlaceRecuperacion
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

                client.Send(message);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail($"No se pudo enviar el correo electronico. Detalle: {ex.Message}");
            }
        }

        private string ConstruirHtmlRecuperacionContrasena(
            string nombres,
            string userName,
            string enlaceRecuperacion)
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
            <h1 style='margin:0; color:white;'>Recuperacion de contraseña</h1>
            <p style='margin:8px 0 0; color:#d1fae5;'>Farmacia VitalCare</p>
        </td>
        </tr>

        <tr>
        <td style='padding:40px;'>

        <p style='font-size:16px;'>{saludo},</p>

        <p style='color:#4b5563; line-height:1.6;'>
        Hemos recibido una solicitud para recuperar tu contraseña. Haz clic en el boton de abajo para cambiarla:
        </p>

        <table align='center' style='margin:30px auto;'>
        <tr>
        <td style='background:#d97706; border-radius:8px;'>

        <a href='{enlaceRecuperacion}'
        style='display:inline-block; padding:14px 26px; color:white; text-decoration:none; font-weight:bold;'>
        Cambiar contraseña
        </a>

        </td>
        </tr>
        </table>

        <p style='color:#4b5563; font-size:13px;'>
        Este enlace es valido por 60 minutos.
        </p>

        <p style='color:#6b7280; font-size:13px;'>
        Si no solicitaste cambiar tu contraseña, ignora este correo. Tu contraseña seguira siendo segura.
        </p>

        <p style='font-size:13px; color:#6b7280;'>
        Si el boton no funciona:
        </p>

        <p style='font-size:12px; word-break:break-all;'>
        <a href='{enlaceRecuperacion}' style='color:#1f7a63;'>{enlaceRecuperacion}</a>
        </p>

        <p>Atentamente,<br><strong>Farmacia VitalCare</strong></p>

        </td>
        </tr>

        <tr>
        <td style='padding:20px; text-align:center; background:#ecfdf5; font-size:12px; color:#6b7280;'>
        Mensaje automatico - no responder
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
