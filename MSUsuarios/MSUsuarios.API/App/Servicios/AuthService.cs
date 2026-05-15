using ServicioUsuarios.App.DTOs;
using ServicioUsuarios.App.Interfaces;
using ServicioUsuarios.Dominio.Modelos;
using ServicioUsuarios.Dominio.Puertos.PuertoSalida;
using ServicioUsuarios.Dominio.Validadores;
using ServicioUsuarios.Infraestructura.Ayudadores;

namespace ServicioUsuarios.App.Servicios
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUsuarioRepository usuarioRepository, ITokenService tokenService)
        {
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
        }

        public Result IniciarSesion(UsuarioLoginRequestDto dto, out UsuarioLoginResponseDto? respuesta)
        {
            respuesta = null;

            Result validacion = ValidarLoginDto(dto);
            if (!validacion.IsSuccess)
                return validacion;

            string emailOUserName = dto.EmailOUserName.Trim();
            Usuario? usuario = _usuarioRepository.GetByEmail(emailOUserName);

            if (usuario == null)
                usuario = _usuarioRepository.GetByUserName(emailOUserName);

            if (usuario == null)
                return Result.Fail("Usuario no existe.");

            if (usuario.Activo != 1)
                return Result.Fail("Usuario inactivo.");

            if (!PasswordHelper.Verify(dto.Password, usuario.PasswordHash))
                return Result.Fail("Credenciales invalidas.");

            UsuarioTokenGeneracionDto tokenGeneracionDto = new UsuarioTokenGeneracionDto
            {
                IdUsuario = usuario.IdUsuario,
                TipoToken = "INICIO_SESION",
                MinutosExpiracion = 60,
                UserName = usuario.UserName,
                Role = usuario.Role
            };

            (Result resultadoToken, string tokenGenerado) = _tokenService.GenerarToken(tokenGeneracionDto, out string _);
            if (!resultadoToken.IsSuccess)
                return resultadoToken;

            respuesta = new UsuarioLoginResponseDto
            {
                IdUsuario = usuario.IdUsuario,
                UserName = usuario.UserName,
                Role = usuario.Role,
                MustChangePassword = usuario.MustChangePassword == 1,
                Token = tokenGenerado,
                ExpiraEn = tokenGeneracionDto.MinutosExpiracion
            };

            return Result.Ok();
        }

        private Result ValidarLoginDto(UsuarioLoginRequestDto dto)
        {
            if (dto == null)
                return Result.Fail("Los datos de inicio de sesion son requeridos.");

            if (string.IsNullOrWhiteSpace(dto.EmailOUserName))
                return Result.Fail("El email o nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return Result.Fail("La contrasena es obligatoria.");

            return Result.Ok();
        }
    }
}
