using System.Data;
using MSVentas.App.DTOs;
using MSVentas.Dominio.Validadores;

namespace MSVentas.App.Interfaces
{
    public interface IUsuarioService
    {
        Result CrearUsuario(UsuarioRegistroDto dto, string role, int? idUsuarioSesion);
        Result ActualizarUsuario(UsuarioActualizarDto dto, int? idUsuarioSesion);
        Result EliminarUsuario(int idUsuario, int? idUsuarioSesion);
        UsuarioDto? ObtenerUsuarioPorId(int idUsuario);
        UsuarioDto? ObtenerUsuarioPorEmail(string email);
        UsuarioDto? ObtenerUsuarioPorUserName(string userName);
        DataTable ObtenerTodos();
        DataTable ObtenerTodos(string filtro);
        Result ValidarActivacionCuenta(string token);
        Result ActivarCuenta(string token, string nuevaPassword);
    }
}

