using ServicioUsuarios.App.DTOs;
using ServicioUsuarios.Dominio.Modelos;
using ServicioUsuarios.Dominio.Validadores;

namespace ServicioUsuarios.App.Interfaces
{
    public interface ITokenService
    {
        (Result, string) GenerarToken(UsuarioTokenGeneracionDto dto, out string tokenPlano);
        UsuarioToken? ValidarToken(string tokenPlano, string tipoToken);
        Result MarcarComoUsado(int idUsuarioToken);
        Result RevocarTokensActivos(int idUsuario, string tipoToken);
        Result EliminarTokensObsoletos(int dias);
    }
}
