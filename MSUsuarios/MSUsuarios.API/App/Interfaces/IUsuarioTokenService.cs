using MSUsuarios.App.DTOs;
using MSUsuarios.Dominio.Modelos;
using MSUsuarios.Dominio.Validadores;

namespace MSUsuarios.App.Interfaces
{
    public interface IUsuarioTokenService
    {
        Result GenerarToken(UsuarioTokenGeneracionDto dto, out string tokenPlano);
        UsuarioToken? ValidarToken(string tokenPlano, string tipoToken);
        Result MarcarComoUsado(int idUsuarioToken);
        Result RevocarTokensActivos(int idUsuario, string tipoToken);
        Result EliminarTokensObsoletos(int dias);
    }
}
