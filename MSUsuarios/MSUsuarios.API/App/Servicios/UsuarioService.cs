using MSUsuarios.App.DTOs;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;

namespace MSUsuarios.App.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly UsuarioValidacionGeneral _validacionGeneral;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public UsuarioService(
            IUsuarioRepository repository,
            UsuarioValidacionGeneral validacionGeneral,
            ITokenService tokenService,
            IEmailService emailService)
        {
            _repository = repository;
            _validacionGeneral = validacionGeneral;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public Result CrearUsuario(UsuarioRegistroDto dto, string role, int? idUsuarioSesion)
        {
            Result validacion = _validacionGeneral.ValidarRegistro(dto);
            if (!validacion.IsSuccess)
                return validacion;

            string passwordTemporal = StringHelper.Limpiar(dto.Password);
            string passwordHash = PasswordHelper.Hash(passwordTemporal);

            Usuario usuario = ConstruirUsuarioNuevo(dto, role, passwordHash, idUsuarioSesion);

            int filasAfectadas = _repository.Insert(usuario);
            if (filasAfectadas <= 0)
                return Result.Fail("No se pudo registrar el usuario.");

            Usuario? usuarioRegistrado = _repository.GetByEmail(usuario.Email);
            if (usuarioRegistrado == null)
                return Result.Fail("El usuario fue registrado, pero no se pudo recuperar su informacion.");

            UsuarioTokenGeneracionDto tokenDto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = usuarioRegistrado.IdUsuario,
                TipoToken = TipoTokenConstantes.ActivacionCuenta,
                MinutosExpiracion = 60
            };

            (Result resultadoToken, string tokenParaUrl) = _tokenService.GenerarToken(tokenDto, out string _);
            if (!resultadoToken.IsSuccess)
                return resultadoToken;

            string tokenSeguro = Uri.EscapeDataString(tokenParaUrl);
            string enlaceActivacion = $"http://localhost:5051/Auth/ActivarCuenta?token={tokenSeguro}";

            return _emailService.EnviarCorreoActivacionCuenta(
                usuarioRegistrado.Email,
                usuarioRegistrado.Nombres,
                usuarioRegistrado.UserName,
                passwordTemporal,
                enlaceActivacion
            );
        }

        public Result ActualizarUsuario(UsuarioActualizarDto dto, int? idUsuarioSesion)
        {
            Result validacion = _validacionGeneral.ValidarActualizacion(dto);
            if (!validacion.IsSuccess)
                return validacion;

            Usuario? usuarioActual = _repository.GetById(dto.IdUsuario);
            if (usuarioActual == null)
                return Result.Fail("El usuario no existe.");

            AplicarActualizacion(usuarioActual, dto);

            int filasAfectadas = _repository.Update(usuarioActual, idUsuarioSesion);
            return filasAfectadas > 0
                ? Result.Ok()
                : Result.Fail("No se pudo actualizar el usuario.");
        }

        public Result EliminarUsuario(int idUsuario, int? idUsuarioSesion)
        {
            Result validacion = _validacionGeneral.ValidarEliminacion(idUsuario);
            if (!validacion.IsSuccess)
                return validacion;

            Usuario? usuario = _repository.GetById(idUsuario);
            if (usuario == null)
                return Result.Fail("El usuario no existe.");

            int filasAfectadas = _repository.SoftDelete(usuario, idUsuarioSesion);
            return filasAfectadas > 0
                ? Result.Ok()
                : Result.Fail("No se pudo eliminar el usuario.");
        }

        public UsuarioDto? ObtenerUsuarioPorId(int idUsuario)
        {
            if (idUsuario <= 0)
                return null;

            return ObtenerYMapear(() => _repository.GetById(idUsuario));
        }

        public UsuarioDto? ObtenerUsuarioPorEmail(string email)
        {
            email = StringHelper.LimpiarTextoMinus(email);
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return ObtenerYMapear(() => _repository.GetByEmail(email));
        }

        public UsuarioDto? ObtenerUsuarioPorUserName(string userName)
        {
            userName = StringHelper.LimpiarTexto(userName);
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            return ObtenerYMapear(() => _repository.GetByUserName(userName));
        }

        public IEnumerable<UsuarioDto> ObtenerTodos()
        {
            return _repository.GetAll().Select(MapearDto);
        }

        public IEnumerable<UsuarioDto> ObtenerTodos(string filtro)
        {
            return _repository.GetAll(StringHelper.LimpiarTexto(filtro)).Select(MapearDto);
        }

        public Result ValidarActivacionCuenta(string token)
        {
            token = token?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token))
                return Result.Fail("El token de activacion es invalido.");

            UsuarioToken? usuarioToken = _tokenService.ValidarToken(token, TipoTokenConstantes.ActivacionCuenta);
            if (usuarioToken == null)
                return Result.Fail("El token ha expirado o es invalido.");

            return Result.Ok();
        }

        public Result ActivarCuenta(string token, string nuevaPassword)
        {
            token = token?.Trim() ?? string.Empty;
            nuevaPassword = nuevaPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(token))
                return Result.Fail("El token de activacion es invalido.");

            if (string.IsNullOrWhiteSpace(nuevaPassword))
                return Result.Fail("La nueva contrasena es obligatoria.");

            if (nuevaPassword.Length < 8)
                return Result.Fail("La contrasena debe tener al menos 8 caracteres.");

            // Validar que tenga mayúsculas, minúsculas y números
            if (!System.Text.RegularExpressions.Regex.IsMatch(nuevaPassword, @"[a-z]"))
                return Result.Fail("La contrasena debe contener al menos una letra minuscula.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(nuevaPassword, @"[A-Z]"))
                return Result.Fail("La contrasena debe contener al menos una letra mayuscula.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(nuevaPassword, @"[0-9]"))
                return Result.Fail("La contrasena debe contener al menos un numero.");

            UsuarioToken? usuarioToken = _tokenService.ValidarToken(token, TipoTokenConstantes.ActivacionCuenta);
            if (usuarioToken == null)
                return Result.Fail("El token ha expirado o es invalido.");

            Usuario? usuario = _repository.GetById(usuarioToken.UsuarioIdUsuario);
            if (usuario == null)
                return Result.Fail("El usuario no existe.");

            string passwordHash = PasswordHelper.Hash(nuevaPassword);
            int filasAfectadas = _repository.CambiarPassword(usuario.IdUsuario, passwordHash, false);
            if (filasAfectadas <= 0)
                return Result.Fail("No se pudo actualizar la contrasena.");

            Result resultadoToken = _tokenService.MarcarComoUsado(usuarioToken.IdUsuarioToken);
            if (!resultadoToken.IsSuccess)
                return resultadoToken;

            return Result.Ok();
        }

        public Result SolicitarRecuperacionContrasena(string email)
        {
            email = StringHelper.LimpiarTextoMinus(email);
            if (string.IsNullOrWhiteSpace(email))
                return Result.Fail("El correo electronico es obligatorio.");

            Usuario? usuario = _repository.GetByEmail(email);
            if (usuario == null)
                return Result.Fail("El correo no esta registrado en el sistema.");

            if (usuario.Activo != 1)
                return Result.Fail("El usuario esta inactivo. Contacta al administrador.");

            UsuarioTokenGeneracionDto tokenDto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = usuario.IdUsuario,
                TipoToken = TipoTokenConstantes.ResetPassword,
                MinutosExpiracion = 60
            };

            (Result resultadoToken, string tokenParaUrl) = _tokenService.GenerarToken(tokenDto, out string _);
            if (!resultadoToken.IsSuccess)
                return resultadoToken;

            string tokenSeguro = Uri.EscapeDataString(tokenParaUrl);
            string enlaceRecuperacion = $"http://localhost:5051/Auth/RecuperarContrasena?token={tokenSeguro}";

            return _emailService.EnviarCorreoRecuperacionContrasena(
                usuario.Email,
                usuario.Nombres,
                usuario.UserName,
                enlaceRecuperacion
            );
        }

        public Result ValidarRecuperacionContrasena(string token)
        {
            token = token?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token))
                return Result.Fail("El token de recuperacion es invalido.");

            UsuarioToken? usuarioToken = _tokenService.ValidarToken(token, TipoTokenConstantes.ResetPassword);
            if (usuarioToken == null)
                return Result.Fail("El token ha expirado o es invalido.");

            return Result.Ok();
        }

        public Result ConfirmarRecuperacionContrasena(string token, string nuevaPassword)
        {
            token = token?.Trim() ?? string.Empty;
            nuevaPassword = nuevaPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(token))
                return Result.Fail("El token de recuperacion es invalido.");

            if (string.IsNullOrWhiteSpace(nuevaPassword))
                return Result.Fail("La nueva contrasena es obligatoria.");

            if (nuevaPassword.Length < 8)
                return Result.Fail("La contrasena debe tener al menos 8 caracteres.");

            // Validar que tenga mayúsculas, minúsculas y números
            if (!System.Text.RegularExpressions.Regex.IsMatch(nuevaPassword, @"[a-z]"))
                return Result.Fail("La contrasena debe contener al menos una letra minuscula.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(nuevaPassword, @"[A-Z]"))
                return Result.Fail("La contrasena debe contener al menos una letra mayuscula.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(nuevaPassword, @"[0-9]"))
                return Result.Fail("La contrasena debe contener al menos un numero.");

            UsuarioToken? usuarioToken = _tokenService.ValidarToken(token, TipoTokenConstantes.ResetPassword);
            if (usuarioToken == null)
                return Result.Fail("El token ha expirado o es invalido.");

            Usuario? usuario = _repository.GetById(usuarioToken.UsuarioIdUsuario);
            if (usuario == null)
                return Result.Fail("El usuario no existe.");

            string passwordHash = PasswordHelper.Hash(nuevaPassword);
            int filasAfectadas = _repository.CambiarPassword(usuario.IdUsuario, passwordHash, false);
            if (filasAfectadas <= 0)
                return Result.Fail("No se pudo actualizar la contrasena.");

            Result resultadoToken = _tokenService.MarcarComoUsado(usuarioToken.IdUsuarioToken);
            if (!resultadoToken.IsSuccess)
                return resultadoToken;

            return Result.Ok();
        }

        private Usuario ConstruirUsuarioNuevo(UsuarioRegistroDto dto, string role, string passwordHash, int? idUsuarioSesion)
        {
            return new Usuario
            {
                Nombres = dto.Nombres,
                ApellidoPaterno = dto.ApellidoPaterno,
                ApellidoMaterno = dto.ApellidoMaterno,
                Ci = dto.Ci,
                CiExtencion = dto.CiExtencion,
                Telefono = dto.Telefono,
                Email = dto.Email,
                UserName = dto.UserName,
                PasswordHash = passwordHash,
                Role = StringHelper.LimpiarTexto(role),
                Activo = 1,
                MustChangePassword = 1,
                IdUsuarioCreador = idUsuarioSesion
            };
        }

        private void AplicarActualizacion(Usuario usuario, UsuarioActualizarDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Nombres))
                usuario.Nombres = dto.Nombres;

            usuario.ApellidoPaterno = dto.ApellidoPaterno;
            usuario.ApellidoMaterno = dto.ApellidoMaterno;
            usuario.Ci = dto.Ci;
            usuario.CiExtencion = dto.CiExtencion;
            usuario.Telefono = dto.Telefono;
            usuario.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Role))
                usuario.Role = dto.Role;
        }

        private UsuarioDto? ObtenerYMapear(Func<Usuario?> obtenerUsuario)
        {
            Usuario? usuario = obtenerUsuario();
            return usuario == null ? null : MapearDto(usuario);
        }

        private static UsuarioDto MapearDto(Usuario usuario)
        {
            return new UsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombres = usuario.Nombres,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                Ci = usuario.Ci,
                CiExtencion = usuario.CiExtencion,
                Telefono = usuario.Telefono,
                Activo = usuario.Activo,
                Email = usuario.Email,
                UserName = usuario.UserName,
                Role = usuario.Role,
                MustChangePassword = usuario.MustChangePassword
            };
        }

    }
}
