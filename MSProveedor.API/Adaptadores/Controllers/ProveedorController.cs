using Microsoft.AspNetCore.Mvc;
using MSProveedor.Aplicacion.DTOs;
using MSProveedor.Aplicacion.InputPorts;

namespace MSProveedor.API.Adaptadores.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProveedorController : ControllerBase
{
    private readonly IProveedorInputPort _interactor;

    public ProveedorController(IProveedorInputPort interactor)
    {
        _interactor = interactor;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProveedorCreateDto dto)
    {
        var resultado = await _interactor.CrearProveedorAsync(dto);

        if (!resultado.Success)
            return BadRequest(new { Error = resultado.Message });

        return Ok(new { Mensaje = resultado.Message, IdProveedor = resultado.Data });
    }
}