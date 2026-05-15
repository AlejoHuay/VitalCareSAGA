using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.App.Interfaces
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
