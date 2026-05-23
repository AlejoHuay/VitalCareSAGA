using MSUsuarios.App.DTOs;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.App.Interfaces
{
    public interface IUsuarioService
    {
        Result CrearUsuario(UsuarioRegistroDto dto, string role, int? idUsuarioSesion);
        Result ActualizarUsuario(UsuarioActualizarDto dto, int? idUsuarioSesion);
        Result EliminarUsuario(int idUsuario, int? idUsuarioSesion);
        Result CambiarPassword(int idUsuario, string passwordActual, string nuevaPassword);
        UsuarioDto? ObtenerUsuarioPorId(int idUsuario);
        UsuarioDto? ObtenerUsuarioPorEmail(string email);
        UsuarioDto? ObtenerUsuarioPorUserName(string userName);
        IEnumerable<UsuarioDto> ObtenerTodos();
        IEnumerable<UsuarioDto> ObtenerTodos(string filtro);
    }
}
