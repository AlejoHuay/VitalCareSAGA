using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSUsuarios.App.DTOs;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;

namespace MSUsuarios.Infraestructura.Adaptadores.PuertosEntrada.Controladores
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioService _usuarioService;
        private readonly IUsuarioTokenService _usuarioTokenService;

        public AuthController(
            IAuthService authService,
            IUsuarioService usuarioService,
            IUsuarioTokenService usuarioTokenService)
        {
            _authService = authService;
            _usuarioService = usuarioService;
            _usuarioTokenService = usuarioTokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UsuarioLoginRequestDto dto)
        {
            Result resultado = _authService.IniciarSesion(dto, out UsuarioLoginResponseDto? respuesta);

            if (!resultado.IsSuccess || respuesta == null)
                return Unauthorized(new { mensaje = resultado.Error });

            return Ok(respuesta);
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] UsuarioRegistroDto dto)
        {
            string role = "Bioquimico";

            dto.UserName = CredencialesHelper.GenerarUserName(
                dto.Nombres,
                dto.ApellidoPaterno,
                dto.Ci
            );

            dto.Password = CredencialesHelper.GenerarPasswordTemporal();

            Result resultado = _usuarioService.CrearUsuario(dto, role, null);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            string mensaje = !string.IsNullOrWhiteSpace(resultado.Error)
                ? resultado.Error
                : "Usuario registrado correctamente. Revisa tu correo electronico para obtener tus credenciales de acceso.";

            return Ok(new { mensaje = mensaje });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            string? idUsuarioValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioValue, out int idUsuario))
                return Unauthorized(new { mensaje = "Token invalido." });

            Result resultado = _usuarioTokenService.RevocarTokensActivos(idUsuario, "INICIO_SESION");
            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Sesión cerrada correctamente." });
        }

        [Authorize]
        [HttpPost("cambiar-contrasena")]
        public IActionResult CambiarContrasena(
            [FromForm] string passwordActual,
            [FromForm] string nuevaPassword,
            [FromForm] string confirmarPassword)
        {
            string? idUsuarioValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idUsuarioValue, out int idUsuario))
                return Unauthorized(new { mensaje = "Token invalido." });

            passwordActual = passwordActual?.Trim() ?? string.Empty;
            nuevaPassword = nuevaPassword?.Trim() ?? string.Empty;
            confirmarPassword = confirmarPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(passwordActual))
                return BadRequest(new { mensaje = "La contraseña actual es obligatoria." });

            if (string.IsNullOrWhiteSpace(nuevaPassword))
                return BadRequest(new { mensaje = "La nueva contrasena es obligatoria." });

            if (nuevaPassword != confirmarPassword)
                return BadRequest(new { mensaje = "La contrasena y su confirmacion no coinciden." });

            if (passwordActual == nuevaPassword)
                return BadRequest(new { mensaje = "La nueva contrasena debe ser diferente a la actual." });

            Result resultado = _usuarioService.CambiarPassword(idUsuario, passwordActual, nuevaPassword);
            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }
    }
}
