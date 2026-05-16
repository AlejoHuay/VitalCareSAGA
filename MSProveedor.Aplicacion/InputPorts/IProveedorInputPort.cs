using MSProveedor.Aplicacion.DTOs;
using MSProveedor.Dominio.Validadores;

namespace MSProveedor.Aplicacion.InputPorts;

public interface IProveedorInputPort
{
    Task<Result<int>> CrearProveedorAsync(ProveedorCreateDto dto);
}