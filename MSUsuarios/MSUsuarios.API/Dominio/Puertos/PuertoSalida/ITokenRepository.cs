
using MSUsuarios.Dominio.Modelos; 

namespace MSUsuarios.Dominio.Puertos.PuertoSalida
{
    public interface ITokenRepository 
    {
        int Insert(Token token);
        Token? GetByTokenHash(string tokenHash);

        Token? GetTokenActivo(string tokenHash, string tipoToken);

        int MarcarComoUsado(int idUsuarioToken);

        int RevocarToken(int idUsuarioToken);

        int EliminarTokensObsoletos(int dias);
    }
}
