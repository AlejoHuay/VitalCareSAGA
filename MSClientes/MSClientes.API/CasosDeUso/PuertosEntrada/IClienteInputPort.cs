using MSClientes.API.Entidades;

namespace MSClientes.API.CasosDeUso.PuertosEntrada
{
    public interface IClienteInputPort
    {
        IEnumerable<Cliente> ObtenerTodos(string filtro);
        Cliente? ObtenerPorId(int id);
        (bool Exito, string Mensaje) Crear(Cliente cliente);
        (bool Exito, string Mensaje) Actualizar(int id, Cliente cliente);
        (bool Exito, string Mensaje) Eliminar(int id, int idUsuario);
    }
}
