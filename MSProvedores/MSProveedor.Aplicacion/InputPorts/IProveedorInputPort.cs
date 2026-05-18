using MSProveedor.Aplicacion.DTOs;
using MSProveedor.Dominio.Entidades;
using MSProveedor.Dominio.Validadores;

namespace MSProveedor.Aplicacion.InputPorts;

public interface IProveedorInputPort
{
    Task<Result<int>> CrearProveedorAsync(ProveedorCreateDto dto);
    Task<Result<IEnumerable<Proveedor>>> ObtenerTodosAsync();
    Task<Result<Proveedor>> ObtenerPorIdAsync(int id);
    Task<Result<bool>> ActualizarProveedorAsync(int id, ProveedorCreateDto dto);
    Task<Result<bool>> EliminarProveedorAsync(int id);
}