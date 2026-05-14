using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Persistencia.Repositorios;

namespace MSUsuarios.Infraestructura.Creadores
{
    public class TokenRepositoryCreator
    {
        public ITokenRepository CreateRepo()
        {
            return new TokenRepository();
        }
    }
}
