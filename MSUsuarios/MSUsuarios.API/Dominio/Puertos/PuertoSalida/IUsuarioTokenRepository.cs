using MSUsuarios.Dominio.Modelos;

namespace MSUsuarios.Dominio.Puertos.PuertoSalida
{
    public interface IUsuarioTokenRepository
    {
        int Insert(UsuarioToken token);
        UsuarioToken? GetByTokenHash(string tokenHash);
        UsuarioToken? GetTokenActivo(string tokenHash, string tipoToken);
        int MarcarComoUsado(int idUsuarioToken);
        int RevocarTokensActivos(int idUsuario, string tipoToken);
        int EliminarTokensObsoletos(int dias);
    }
}
