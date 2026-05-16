using MSProveedor.Aplicacion.DTOs;
using MSProveedor.Aplicacion.InputPorts;
using MSProveedor.Dominio.Entidades;
using MSProveedor.Dominio.Interfaces;
using MSProveedor.Dominio.Validadores;

namespace MSProveedor.Aplicacion.Interactors;

public class ProveedorInteractor : IProveedorInputPort
{
    private readonly IProveedorRepository _repository;

    public ProveedorInteractor(IProveedorRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<int>> CrearProveedorAsync(ProveedorCreateDto dto)
    {
        try
        {
            // Validación de negocio
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return Result<int>.Falla("El nombre del proveedor es obligatorio.");

            if (await _repository.ExisteNombreAsync(dto.Nombre))
                return Result<int>.Falla("Ya existe un proveedor con ese nombre.");

            // Mapeo
            var entidad = new Proveedor
            {
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                CorreoElectronico = dto.CorreoElectronico,
                Direccion = dto.Direccion,
                IdUsuario = dto.IdUsuario
            };

            // Guardar
            int idGenerado = await _repository.CrearAsync(entidad);
            
            return Result<int>.Exito(idGenerado, "Proveedor creado correctamente.");
        }
        catch (Exception ex)
        {
            return Result<int>.Falla($"Error interno: {ex.Message}");
        }
    }
}