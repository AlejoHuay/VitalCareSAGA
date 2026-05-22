using MSUsuarios.Dominio.Modelos;

namespace MSUsuarios.Dominio.Puertos.PuertoSalida
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        int Update(Usuario usuario, int? idUsuarioSesion);
        Usuario? GetByCi(string ci);
        Usuario? GetByEmail(string email);
        Usuario? GetByUserName(string userName);
        bool ExisteCi(string ci);
        bool ExisteEmail(string email);
        bool ExisteUserName(string userName);
        int CambiarPassword(int idUsuario, string nuevoPasswordHash, bool mustChangePassword);
        int UpdateDatosEdicion(Usuario usuario, int? idUsuarioSesion);
        int Count();
        int SoftDelete(Usuario usuario, int? idUsuarioSesion);
    }
}
