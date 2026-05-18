using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Persistencia.Repositorios;

namespace MSUsuarios.Infraestructura.Creadores
{
    public class UsuarioTokenRepositoryCreator
    {
        public IUsuarioTokenRepository CreateRepo()
        {
            return new UsuarioTokenRepository();
        }
    }
}
