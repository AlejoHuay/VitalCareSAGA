using MSProveedor.Aplicacion.DTOs;
using MSProveedor.Aplicacion.InputPorts;
using MSProveedor.Dominio.Entidades;
using MSProveedor.Dominio.Interfaces;
using MSProveedor.Dominio.Validadores;

namespace MSProveedor.Aplicacion.Interactors;

public class ProveedorInteractor : IProveedorInputPort
{
    private readonly IProveedorRepository _repository;

    public ProveedorInteractor(IProveedorRepository repository) => _repository = repository;

    public async Task<Result<int>> CrearProveedorAsync(ProveedorCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre)) return Result<int>.Falla("El nombre es obligatorio.");
        if (await _repository.ExisteNombreAsync(dto.Nombre)) return Result<int>.Falla("El nombre ya existe.");

        var entidad = new Proveedor { Nombre = dto.Nombre, Telefono = dto.Telefono, CorreoElectronico = dto.CorreoElectronico, Direccion = dto.Direccion, IdUsuario = dto.IdUsuario };
        int idGenerado = await _repository.CrearAsync(entidad);
        return Result<int>.Exito(idGenerado, "Proveedor creado.");
    }

    public async Task<Result<IEnumerable<Proveedor>>> ObtenerTodosAsync()
    {
        var lista = await _repository.ObtenerTodosAsync();
        return Result<IEnumerable<Proveedor>>.Exito(lista);
    }

    public async Task<Result<Proveedor>> ObtenerPorIdAsync(int id)
    {
        var proveedor = await _repository.ObtenerPorIdAsync(id);
        if (proveedor == null) return Result<Proveedor>.Falla("Proveedor no encontrado.");
        return Result<Proveedor>.Exito(proveedor);
    }

    public async Task<Result<bool>> ActualizarProveedorAsync(int id, ProveedorCreateDto dto)
    {
        var existe = await _repository.ObtenerPorIdAsync(id);
        if (existe == null) return Result<bool>.Falla("Proveedor no encontrado.");

        existe.Nombre = dto.Nombre;
        existe.Telefono = dto.Telefono;
        existe.CorreoElectronico = dto.CorreoElectronico;
        existe.Direccion = dto.Direccion;
        existe.IdUsuario = dto.IdUsuario;

        await _repository.ActualizarAsync(existe);
        return Result<bool>.Exito(true, "Proveedor actualizado.");
    }

    public async Task<Result<bool>> EliminarProveedorAsync(int id)
    {
        var existe = await _repository.ObtenerPorIdAsync(id);
        if (existe == null) return Result<bool>.Falla("Proveedor no encontrado.");

        await _repository.EliminarAsync(id);
        return Result<bool>.Exito(true, "Proveedor eliminado.");
    }
}