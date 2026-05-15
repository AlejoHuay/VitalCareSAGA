using ServicioUsuarios.App.DTOs;
using ServicioUsuarios.Dominio.Validadores;

namespace ServicioUsuarios.App.Interfaces
{
    public interface IAuthService
    {
        Result IniciarSesion(UsuarioLoginRequestDto dto, out UsuarioLoginResponseDto? respuesta);
    }
}
