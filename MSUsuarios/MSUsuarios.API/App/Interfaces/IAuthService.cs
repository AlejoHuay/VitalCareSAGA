using MSUsuarios.App.DTOs;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.App.Interfaces
{
    public interface IAuthService
    {
        Result IniciarSesion(UsuarioLoginRequestDto dto, out UsuarioLoginResponseDto? respuesta);
    }
}
