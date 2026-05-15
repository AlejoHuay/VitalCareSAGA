using ServicioUsuarios.Dominio.Validadores;

namespace ServicioUsuarios.App.Interfaces
{
    public interface IEmailService
    {
        Result EnviarCorreoActivacionCuenta(
            string emailDestino,
            string nombres,
            string userName,
            string passwordTemporal,
            string enlaceActivacion
        );

        Result EnviarCorreoRecuperacionContrasena(
            string emailDestino,
            string nombres,
            string userName,
            string enlaceRecuperacion
        );
    }
}
