using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSUsuarios.App.DTOs;
using MSUsuarios.App.Interfaces;
using MSUsuarios.Dominio.Validadores;
using MSUsuarios.Infraestructura.Ayudadores;

namespace MSUsuarios.Infraestructura.Adaptadores.PuertosEntrada.Controladores
{
    [ApiController]
    [Authorize]
    [Route("api/usuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet("GetUsers")]
        public IActionResult GetAllUsers([FromQuery] string? filtro)
        {
            IEnumerable<UsuarioDto> usuarios = string.IsNullOrWhiteSpace(filtro)
                ? _usuarioService.ObtenerTodos()
                : _usuarioService.ObtenerTodos(filtro);
            
            return Ok(new { mensaje = "Usuarios obtenidos correctamente.", data = usuarios });
        }

        [HttpGet("getUser")]
        public IActionResult GetOneUser([FromQuery]  string? email, [FromQuery] string? userName)
        {
            UsuarioDto? usuario = null; 
            string email_name = email?.Trim() ?? string.Empty;
            string userName_name = userName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email_name) && string.IsNullOrWhiteSpace(userName_name))
                return BadRequest(new { mensaje = "Debe proporcionar un email o un userName." });

            usuario = !string.IsNullOrWhiteSpace(email_name)
            ?_usuarioService.ObtenerUsuarioPorEmail(email_name)
            : _usuarioService.ObtenerUsuarioPorUserName(userName_name);
            return usuario == null?
            BadRequest(new { mensaje = "Usuario no encontrado." ,StatusCode = 404}):
            Ok(new { mensaje = "Usuario obtenido correctamente.", data = usuario });
        }

        [HttpGet("getUserById")]
        public IActionResult GetUserById([FromQuery] string id)
        {
            if (!int.TryParse(id, out int idUsuario))
                return BadRequest(new { mensaje = "Id de usuario invalido.", StatusCode = 400 });

            UsuarioDto? usuario = _usuarioService.ObtenerUsuarioPorId(idUsuario);

            return usuario == null
                ? BadRequest(new { mensaje = "Usuario no encontrado.", StatusCode = 404 })
                : Ok(new { mensaje = "Usuario obtenido correctamente.", data = usuario });
        }


        [HttpPost("CrearUsuario")]
        public IActionResult CrearUsuario([FromBody] UsuarioRegistroDto dto)
        {
            dto.UserName = CredencialesHelper.GenerarUserName(
                dto.Nombres,
                dto.ApellidoPaterno,
                dto.Ci
            );

            dto.Password = CredencialesHelper.GenerarPasswordTemporal();

             Result resultado = _usuarioService.CrearUsuario(dto, dto.Role?? "Bioquiimico", null);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error, StatusCode = 400 });

            return Ok(new
            {
                mensaje = "Usuario registrado correctamente. Revisa tu correo electronico para activar la cuenta.", StatusCode = 201
            });
        } 
        [HttpDelete("EliminarUsuario")]
        public IActionResult EliminarUsuario([FromQuery] string idUsuario, [FromQuery] string idUsuarioSesion)
        {
            if (!int.TryParse(idUsuario, out int idUsuarioInt))
                return BadRequest(new { mensaje = "Id de usuario invalido.", StatusCode = 400 });

            int? idUsuarioSesionInt = null;
            if (!string.IsNullOrWhiteSpace(idUsuarioSesion))
            {
                if (!int.TryParse(idUsuarioSesion, out int idSesion))
                    return BadRequest(new { mensaje = "Id de usuario de sesion invalido.", StatusCode = 400 });

                idUsuarioSesionInt = idSesion;
            }
             Result resultado = _usuarioService.EliminarUsuario(idUsuarioInt, idUsuarioSesionInt);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error, StatusCode = 400 });

            return Ok(new { mensaje = "Usuario eliminado correctamente.", StatusCode = 204 });

        }

        [HttpPut("actualizarUsuario")]
        public IActionResult ActualizarUsuario([FromBody] UsuarioActualizarDto dto, [FromQuery] string? idUsuarioSesion)
        {
            int? idUsuarioSesionInt = null;
            if (!string.IsNullOrWhiteSpace(idUsuarioSesion))
            {
                if (!int.TryParse(idUsuarioSesion, out int idSesion))
                    return BadRequest(new { mensaje = "Id de usuario de sesion invalido.", StatusCode = 400 });

                idUsuarioSesionInt = idSesion;
            }

             Result resultado = _usuarioService.ActualizarUsuario(dto, idUsuarioSesionInt);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error, StatusCode = 400 });

            return Ok(new { mensaje = "Usuario actualizado correctamente.", StatusCode = 200 });
        }
    }

}

