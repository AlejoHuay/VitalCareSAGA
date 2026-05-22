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
                : "Usuario registrado correctamente. Revisa tu correo electronico para activar la cuenta.";

            return Ok(new { mensaje = mensaje });
        }

        [HttpGet("validar-activacion")]
        public IActionResult ValidarActivacion([FromQuery] string token)
        {
            Result resultado = _usuarioService.ValidarActivacionCuenta(token);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Token valido. Ahora debes definir tu nueva contrasena." });
        }

        [HttpPost("activar-cuenta")]
        public IActionResult ActivarCuenta(
            [FromForm] string token,
            [FromForm] string nuevaPassword,
            [FromForm] string confirmarPassword)
        {
            token = token?.Trim() ?? string.Empty;
            nuevaPassword = nuevaPassword?.Trim() ?? string.Empty;
            confirmarPassword = confirmarPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { mensaje = "Token invalido." });

            if (string.IsNullOrWhiteSpace(nuevaPassword))
                return BadRequest(new { mensaje = "La nueva contrasena es obligatoria." });

            if (nuevaPassword != confirmarPassword)
                return BadRequest(new { mensaje = "La contrasena y su confirmacion no coinciden." });

            Result resultado = _usuarioService.ActivarCuenta(token, nuevaPassword);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Cuenta activada correctamente. Ya puedes iniciar sesion." });
        }

        [HttpPost("solicitar-recuperacion-contrasena")]
        public IActionResult SolicitarRecuperacionContrasena([FromBody] SolicitarRecuperacionDto dto)
        {
            string? email = dto?.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { mensaje = "El correo electronico es obligatorio." });

            Result resultado = _usuarioService.SolicitarRecuperacionContrasena(email);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Se ha enviado un enlace de recuperacion a tu correo electronico." });
        }

        [HttpGet("validar-recuperacion-contrasena")]
        public IActionResult ValidarRecuperacionContrasena([FromQuery] string token)
        {
            Result resultado = _usuarioService.ValidarRecuperacionContrasena(token);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Token valido. Ahora puedes cambiar tu contrasena." });
        }

        [HttpPost("confirmar-recuperacion-contrasena")]
        public IActionResult ConfirmarRecuperacionContrasena(
            [FromForm] string token,
            [FromForm] string nuevaPassword,
            [FromForm] string confirmarPassword)
        {
            token = token?.Trim() ?? string.Empty;
            nuevaPassword = nuevaPassword?.Trim() ?? string.Empty;
            confirmarPassword = confirmarPassword?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { mensaje = "Token invalido." });

            if (string.IsNullOrWhiteSpace(nuevaPassword))
                return BadRequest(new { mensaje = "La nueva contrasena es obligatoria." });

            if (nuevaPassword != confirmarPassword)
                return BadRequest(new { mensaje = "La contrasena y su confirmacion no coinciden." });

            Result resultado = _usuarioService.ConfirmarRecuperacionContrasena(token, nuevaPassword);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Contraseña actualizada correctamente. Ya puedes iniciar sesion con tu nueva contraseña." });
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

            return Ok(new { mensaje = "Sesion cerrada correctamente." });
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
                return BadRequest(new { mensaje = "La contrasena actual es obligatoria." });

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
