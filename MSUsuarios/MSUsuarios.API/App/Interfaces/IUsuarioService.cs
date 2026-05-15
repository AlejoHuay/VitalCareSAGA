using MSUsuarios.App.DTOs;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.App.Interfaces
{
    public interface IUsuarioService
    {
        Result CrearUsuario(UsuarioRegistroDto dto, string role, int? idUsuarioSesion);
        Result ActualizarUsuario(UsuarioActualizarDto dto, int? idUsuarioSesion);
        Result EliminarUsuario(int idUsuario, int? idUsuarioSesion);
        UsuarioDto? ObtenerUsuarioPorId(int idUsuario);
        UsuarioDto? ObtenerUsuarioPorEmail(string email);
        UsuarioDto? ObtenerUsuarioPorUserName(string userName);
        IEnumerable<UsuarioDto> ObtenerTodos();
        IEnumerable<UsuarioDto> ObtenerTodos(string filtro);
        Result ValidarActivacionCuenta(string token);
        Result ActivarCuenta(string token, string nuevaPassword);
        Result SolicitarRecuperacionContrasena(string email);
        Result ValidarRecuperacionContrasena(string token);
        Result ConfirmarRecuperacionContrasena(string token, string nuevaPassword);
    }
}
