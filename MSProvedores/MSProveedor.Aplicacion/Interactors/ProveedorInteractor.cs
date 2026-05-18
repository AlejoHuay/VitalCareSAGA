using MSProveedor.Aplicacion.DTOs;
using MSProveedor.Aplicacion.InputPorts;
using MSProveedor.Dominio.Entidades;
using MSProveedor.Dominio.Interfaces;
using MSProveedor.Dominio.Validadores;
using System.Text.RegularExpressions;

namespace MSProveedor.Aplicacion.Interactors;

public class ProveedorInteractor : IProveedorInputPort
{
    private readonly IProveedorRepository _repository;
    private static readonly Regex NombreRegex = new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled);
    private static readonly Regex TelefonoRegex = new(@"^\d{8}$", RegexOptions.Compiled);

    public ProveedorInteractor(IProveedorRepository repository) => _repository = repository;

    public async Task<Result<int>> CrearProveedorAsync(ProveedorCreateDto dto)
    {
        var validacion = ValidarProveedor(dto);
        if (!validacion.Success) return Result<int>.Falla(validacion.Message);

        dto.Nombre = dto.Nombre.Trim();
        dto.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();

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
        var validacion = ValidarProveedor(dto);
        if (!validacion.Success) return Result<bool>.Falla(validacion.Message);

        var existe = await _repository.ObtenerPorIdAsync(id);
        if (existe == null) return Result<bool>.Falla("Proveedor no encontrado.");

        existe.Nombre = dto.Nombre.Trim();
        existe.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
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

    private static Result<bool> ValidarProveedor(ProveedorCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<bool>.Falla("El nombre es obligatorio.");

        if (!NombreRegex.IsMatch(dto.Nombre.Trim()))
            return Result<bool>.Falla("El nombre solo puede contener letras.");

        if (!string.IsNullOrWhiteSpace(dto.Telefono) && !TelefonoRegex.IsMatch(dto.Telefono.Trim()))
            return Result<bool>.Falla("El telefono debe tener exactamente 8 digitos.");

        return Result<bool>.Exito(true);
    }
}
