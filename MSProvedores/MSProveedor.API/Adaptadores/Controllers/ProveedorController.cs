using Microsoft.AspNetCore.Mvc;
using MSProveedor.Aplicacion.DTOs;
using MSProveedor.Aplicacion.InputPorts;

namespace MSProveedor.API.Adaptadores.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ProveedorController : ControllerBase
{
    private readonly IProveedorInputPort _interactor;

    public ProveedorController(IProveedorInputPort interactor) => _interactor = interactor;

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProveedorCreateDto dto)
    {
        var res = await _interactor.CrearProveedorAsync(dto);
        return res.Success ? Ok(new { Mensaje = res.Message, Id = res.Data }) : BadRequest(new { Error = res.Message });
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var res = await _interactor.ObtenerTodosAsync();
        return Ok(res.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var res = await _interactor.ObtenerPorIdAsync(id);
        return res.Success ? Ok(res.Data) : NotFound(new { Error = res.Message });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ProveedorCreateDto dto)
    {
        var res = await _interactor.ActualizarProveedorAsync(id, dto);
        return res.Success ? Ok(new { Mensaje = res.Message }) : BadRequest(new { Error = res.Message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id, [FromQuery] int idUsuario)
    {
        var res = await _interactor.EliminarProveedorAsync(id, idUsuario);
        return res.Success ? Ok(new { Mensaje = res.Message }) : NotFound(new { Error = res.Message });
    }
}