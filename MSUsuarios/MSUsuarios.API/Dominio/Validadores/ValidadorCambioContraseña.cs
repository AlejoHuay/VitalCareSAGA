using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Infraestructura.Ayudadores;

namespace MSUsuarios.Dominio.Validadores
{
    public class ValidadorCambioContraseña : UsuarioValidacionBase
    {
        private readonly ValidadorContraseña _validadorContraseña;

        public ValidadorCambioContraseña()
        {
            _validadorContraseña = new ValidadorContraseña();
        }

        public Result Validar(
            string? passwordActual,
            string? passwordNueva,
            string? confirmacion,
            Usuario usuario)
        {
            if (usuario == null)
                return Result.Fail("El usuario no existe.");

            passwordActual = passwordActual?.Trim() ?? string.Empty;
            passwordNueva = passwordNueva?.Trim() ?? string.Empty;
            confirmacion = confirmacion?.Trim() ?? string.Empty;

            // Validar que se proporcionó la contraseña actual
            if (string.IsNullOrWhiteSpace(passwordActual))
                return Result.Fail("La contraseña actual es obligatoria.");

            // Validar que la contraseña actual sea correcta
            if (!PasswordHelper.Verify(passwordActual, usuario.PasswordHash))
                return Result.Fail("La contraseña actual es incorrecta.");

            // Validar complejidad de la nueva contraseña
            Result resultadoComplejidad = _validadorContraseña.ValidarComplexidad(passwordNueva);
            if (!resultadoComplejidad.IsSuccess)
                return resultadoComplejidad;

            // Validar que las contraseñas coincidan
            Result resultadoCoincidencia = _validadorContraseña.ValidarCoincidencia(passwordNueva, confirmacion);
            if (!resultadoCoincidencia.IsSuccess)
                return resultadoCoincidencia;

            // Validar que sea diferente a la actual
            Result resultadoDiferencia = _validadorContraseña.ValidarDiferencia(passwordActual, passwordNueva);
            if (!resultadoDiferencia.IsSuccess)
                return resultadoDiferencia;

            return Result.Ok();
        }
    }
}
