using System.Text.RegularExpressions;
using MSUsuarios.App.DTOs;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;

namespace MSUsuarios.Dominio.Validadores
{
    public class UsuarioValidacionGeneral : UsuarioValidacionBase
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioValidacionGeneral(IUsuarioRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Result ValidarRegistro(UsuarioRegistroDto dto)
        {
            if (dto == null)
                return Result.Fail("Los datos del usuario no pueden ser nulos.");

            string? nombres = LimpiarTexto(dto.Nombres);
            string? apellidoPaterno = LimpiarTexto(dto.ApellidoPaterno);
            string? apellidoMaterno = LimpiarTexto(dto.ApellidoMaterno);
            string? ci = LimpiarTexto(dto.Ci);
            string? ciExtension = LimpiarTexto(dto.CiExtencion);
            string? telefono = LimpiarTexto(dto.Telefono);
            string? email = LimpiarTexto(dto.Email);
            string? userName = LimpiarTexto(dto.UserName);
            string? password = LimpiarTexto(dto.Password);

            Result? resultado = ValidarCamposObligatorios(nombres, apellidoPaterno, apellidoMaterno, email)
                ?? ValidarCi(ci)
                ?? ValidarCiExtension(ciExtension)
                ?? ValidarTelefono(telefono)
                ?? ValidarEmail(email)
                ?? ValidarPassword(password)
                ?? ValidarEmailDuplicado(email!)
                ?? ValidarUserNameDuplicado(userName!);

            return resultado ?? Result.Ok();
        }

        public Result ValidarActualizacion(UsuarioActualizarDto dto)
        {
            if (dto == null)
                return Result.Fail("Los datos del usuario no pueden ser nulos.");

            if (dto.IdUsuario <= 0)
                return Result.Fail("El identificador del usuario no es valido.");

            string? nombres = LimpiarTexto(dto.Nombres);
            string? apellidoPaterno = LimpiarTexto(dto.ApellidoPaterno);
            string? apellidoMaterno = LimpiarTexto(dto.ApellidoMaterno);
            string? ci = LimpiarTexto(dto.Ci);
            string? ciExtension = LimpiarTexto(dto.CiExtencion);
            string? telefono = LimpiarTexto(dto.Telefono);
            string? email = LimpiarTexto(dto.Email);

            Result? resultado = ValidarCamposObligatorios(nombres, apellidoPaterno, apellidoMaterno, email)
                ?? ValidarCi(ci)
                ?? ValidarCiExtension(ciExtension)
                ?? ValidarTelefono(telefono)
                ?? ValidarEmail(email)
                ?? ValidarEmailDuplicadoEnActualizacion(dto.IdUsuario, email!)
                ?? ValidarUserNameDuplicadoEnActualizacion(dto.IdUsuario, dto.UserName);

            return resultado ?? Result.Ok();
        }

        public Result ValidarEliminacion(int idUsuario)
        {
            if (idUsuario <= 0)
                return Result.Fail("El identificador del usuario no es valido.");

            Usuario? usuario = _repository.GetById(idUsuario);
            if (usuario == null)
                return Result.Fail("El usuario no existe.");

            return Result.Ok();
        }

        private Result? ValidarCamposObligatorios(string? nombres, string? apellidoPaterno, string? apellidoMaterno, string? email)
        {
            Result? resultado = TextoSoloLetrasRequerido(nombres, "Nombres", 100)
                ?? TextoSoloLetrasRequerido(apellidoPaterno, "Apellido Paterno", 100)
                ?? TextoSoloLetrasOpcional(apellidoMaterno, "Apellido Materno", 100);

            if (resultado != null)
                return resultado;

            if (string.IsNullOrWhiteSpace(email))
                return Result.Fail("El campo Email es obligatorio.");

            return null;
        }

        private Result? ValidarCi(string? ci)
        {
            if (string.IsNullOrWhiteSpace(ci))
                return Result.Fail("El numero de carnet es obligatorio.");

            if (ci!.Contains(' '))
                return Result.Fail("El numero de carnet no debe contener espacios.");

            if (!Regex.IsMatch(ci, @"^\d{8}$"))
                return Result.Fail("El CI debe tener exactamente 8 digitos numericos.");

            return null;
        }

        private Result? ValidarCiExtension(string? ciExtension)
        {
            if (string.IsNullOrWhiteSpace(ciExtension))
                return Result.Fail("La extension del CI es obligatoria.");

            if (!ExtensionesValidas.Contains(ciExtension!))
                return Result.Fail("La extension del CI no es valida.");

            return null;
        }

        private Result? ValidarTelefono(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return Result.Fail("El telefono es obligatorio.");

            if (telefono!.Length != 8)
                return Result.Fail("El telefono debe tener exactamente 8 digitos.");

            if (!Regex.IsMatch(telefono, @"^\d{8}$"))
                return Result.Fail("El telefono debe contener solo digitos numericos.");

            return null;
        }

        private Result? ValidarEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result.Fail("El email es obligatorio.");

            if (!Regex.IsMatch(email!, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return Result.Fail("El formato del email no es valido.");

            if (email!.Length > 255)
                return Result.Fail("El email no puede exceder 255 caracteres.");

            return null;
        }

        private Result? ValidarPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result.Fail("La contrasena es obligatoria.");

            if (password!.Length < 8)
                return Result.Fail("La contrasena debe tener al menos 8 caracteres.");

            if (password.Length > 128)
                return Result.Fail("La contrasena no puede exceder 128 caracteres.");

            if (!Regex.IsMatch(password, @"[a-z]"))
                return Result.Fail("La contrasena debe contener al menos una letra minuscula.");

            if (!Regex.IsMatch(password, @"[A-Z]"))
                return Result.Fail("La contrasena debe contener al menos una letra mayuscula.");

            if (!Regex.IsMatch(password, @"[0-9]"))
                return Result.Fail("La contrasena debe contener al menos un numero.");

            return null;
        }

        private Result? ValidarEmailDuplicado(string email)
        {
            if (_repository.ExisteEmail(email))
                return Result.Fail("El email ya esta registrado en el sistema.");

            return null;
        }

        private Result? ValidarUserNameDuplicado(string userName)
        {
            if (_repository.ExisteUserName(userName))
                return Result.Fail("El nombre de usuario ya esta registrado en el sistema.");

            return null;
        }

        private Result? ValidarEmailDuplicadoEnActualizacion(int idUsuario, string email)
        {
            Usuario? usuario = _repository.GetByEmail(email);
            if (usuario != null && usuario.IdUsuario != idUsuario)
                return Result.Fail("El email ya esta registrado en el sistema.");

            return null;
        }

        private Result? ValidarUserNameDuplicadoEnActualizacion(int idUsuario, string? userName)
        {
            userName = LimpiarTexto(userName);
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            Usuario? usuario = _repository.GetByUserName(userName);
            if (usuario != null && usuario.IdUsuario != idUsuario)
                return Result.Fail("El nombre de usuario ya esta registrado en el sistema.");

            return null;
        }

        private string? LimpiarTexto(string? texto)
        {
            return texto?.Trim();
        }
    }
}
