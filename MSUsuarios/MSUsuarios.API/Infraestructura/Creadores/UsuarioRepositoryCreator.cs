using MSUsuarios.Dominio.Puertos.PuertoSalida;
using MSUsuarios.Infraestructura.Persistencia.Repositorios;

namespace MSUsuarios.Infraestructura.Creadores
{
    public class UsuarioRepositoryCreator
    {
        public IUsuarioRepository CreateRepo()
        {
            return new UsuarioRepository();
        }
    }
}
