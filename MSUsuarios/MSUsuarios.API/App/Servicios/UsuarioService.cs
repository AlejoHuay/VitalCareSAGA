using MSUsuarios.App.DTOs;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace MSUsuarios.App.Servicios
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly UsuarioValidacionGeneral _validacionGeneral;
        private readonly ValidadorContraseña _validadorContraseña;
        private readonly ValidadorCambioContraseña _validadorCambioContraseña;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly string _frontendBaseUrl;

        public UsuarioService(
            IUsuarioRepository repository,
            UsuarioValidacionGeneral validacionGeneral,
            ValidadorContraseña validadorContraseña,
            ValidadorCambioContraseña validadorCambioContraseña,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _repository = repository;
            _validacionGeneral = validacionGeneral;
            _validadorContraseña = validadorContraseña;
            _validadorCambioContraseña = validadorCambioContraseña;
            _tokenService = tokenService;
            _emailService = emailService;
            _frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL")
                ?? configuration["Frontend:BaseUrl"]?.TrimEnd('/')
                ?? "http://localhost:5081";
        }

        public Result CrearUsuario(UsuarioRegistroDto dto, string role, int? idUsuarioSesion)
        {
            try
            {
                Result validacion = _validacionGeneral.ValidarRegistro(dto);
                if (!validacion.IsSuccess)
                    return validacion;

                string passwordTemporal = StringHelper.Limpiar(dto.Password);
                string passwordHash = PasswordHelper.Hash(passwordTemporal);

                Usuario usuario = ConstruirUsuarioNuevo(dto, role, passwordHash, idUsuarioSesion);

                int filasAfectadas;
                try
                {
                    filasAfectadas = _repository.Insert(usuario);
                }
                catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
                {
                    return Result.Fail(MapearViolacionUnica(ex.ConstraintName));
                }

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

                string enlaceActivacion = ConstruirEnlaceFrontend("/Auth/ActivarCuenta", tokenParaUrl);

                Result resultadoEmail = _emailService.EnviarCorreoActivacionCuenta(
                    usuarioRegistrado.Email,
                    usuarioRegistrado.Nombres,
                    usuarioRegistrado.UserName,
                    passwordTemporal,
                    enlaceActivacion
                );

                // El registro es exitoso incluso si el email falla (se notifica en la respuesta)
                if (!resultadoEmail.IsSuccess)
                {
                    // Log the email error pero no falla el registro
                    Console.WriteLine($"Advertencia al enviar email: {resultadoEmail.Error}");
                    return Result.Ok($"Usuario registrado. Nota: No se pudo enviar el correo de activación. {resultadoEmail.Error}");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado en CrearUsuario: {ex.Message}\n{ex.StackTrace}");
                return Result.Fail($"Error al registrar usuario: {ex.Message}");
            }
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

            int filasAfectadas;
            try
            {
                filasAfectadas = _repository.Update(usuarioActual, idUsuarioSesion);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Result.Fail(MapearViolacionUnica(ex.ConstraintName));
            }

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

            // Validar complejidad de la contraseña
            Result resultadoValidacion = _validadorContraseña.ValidarComplexidad(nuevaPassword);
            if (!resultadoValidacion.IsSuccess)
                return resultadoValidacion;

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

            string enlaceRecuperacion = ConstruirEnlaceFrontend("/Auth/RecuperarContrasena", tokenParaUrl);

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

            // Validar complejidad de la contraseña
            Result resultadoValidacion = _validadorContraseña.ValidarComplexidad(nuevaPassword);
            if (!resultadoValidacion.IsSuccess)
                return resultadoValidacion;

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

        public Result CambiarPassword(int idUsuario, string passwordActual, string nuevaPassword)
        {
            Usuario? usuario = _repository.GetById(idUsuario);
            if (usuario == null)
                return Result.Fail("El usuario no existe.");

            // Validar el cambio de contraseña (verifica actual, complejidad, coincidencia, diferencia)
            Result resultadoValidacion = _validadorCambioContraseña.Validar(passwordActual, nuevaPassword, nuevaPassword, usuario);
            if (!resultadoValidacion.IsSuccess)
                return resultadoValidacion;

            string passwordHash = PasswordHelper.Hash(nuevaPassword);
            int filasAfectadas = _repository.CambiarPassword(usuario.IdUsuario, passwordHash, false);
            if (filasAfectadas <= 0)
                return Result.Fail("No se pudo actualizar la contrasena.");

            return Result.Ok();
        }

        private Usuario ConstruirUsuarioNuevo(UsuarioRegistroDto dto, string role, string passwordHash, int? idUsuarioSesion)
        {
            return new Usuario
            {
                Nombres = StringHelper.LimpiarTexto(dto.Nombres),
                ApellidoPaterno = StringHelper.LimpiarTexto(dto.ApellidoPaterno),
                ApellidoMaterno = StringHelper.LimpiarTexto(dto.ApellidoMaterno),
                Ci = StringHelper.SoloNumeros(dto.Ci),
                CiExtencion = StringHelper.LimpiarTextoMayus(dto.CiExtencion),
                Telefono = StringHelper.SoloNumeros(dto.Telefono),
                Email = StringHelper.LimpiarTextoMinus(dto.Email),
                UserName = StringHelper.LimpiarTextoMinus(dto.UserName),
                PasswordHash = passwordHash,
                Role = StringHelper.LimpiarTexto(role),
                Activo = 1,
                MustChangePassword = 1,
                FechaRegistro = DateTime.UtcNow,
                IdUsuarioCreador = idUsuarioSesion
            };
        }

        private void AplicarActualizacion(Usuario usuario, UsuarioActualizarDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Nombres))
                usuario.Nombres = StringHelper.LimpiarTexto(dto.Nombres);

            usuario.ApellidoPaterno = StringHelper.LimpiarTexto(dto.ApellidoPaterno);
            usuario.ApellidoMaterno = StringHelper.LimpiarTexto(dto.ApellidoMaterno);
            usuario.Ci = StringHelper.SoloNumeros(dto.Ci);
            usuario.CiExtencion = StringHelper.LimpiarTextoMayus(dto.CiExtencion);
            usuario.Telefono = StringHelper.SoloNumeros(dto.Telefono);
            usuario.Email = StringHelper.LimpiarTextoMinus(dto.Email);

            if (!string.IsNullOrWhiteSpace(dto.Role))
                usuario.Role = StringHelper.LimpiarTexto(dto.Role);
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

        private string ConstruirEnlaceFrontend(string rutaRelativa, string tokenPlano)
        {
            string tokenSeguro = Uri.EscapeDataString(tokenPlano);
            return $"{_frontendBaseUrl}{rutaRelativa}?token={tokenSeguro}";
        }

        private static string MapearViolacionUnica(string? constraintName)
        {
            return constraintName switch
            {
                "usuario_ci_key" => "El CI ya esta registrado en el sistema.",
                "usuario_email_key" => "El email ya esta registrado en el sistema.",
                "usuario_user_name_key" => "El nombre de usuario ya esta registrado en el sistema.",
                _ => "Ya existe un registro con datos unicos duplicados."
            };
        }
    }
}
