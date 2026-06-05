using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSProductos.CasosDeUso.DTOs;
using MSProductos.CasosDeUso.PuertosEntrada;
using MSProductos.Dominio.Entidades;

namespace MSProductos.API.AdaptadoresEntrada.Controladores
{
    [ApiController]
    [Route("api/medicamentos")]
    [Authorize(Roles = "Admin,Bioquimico")]
    public class MedicamentoController : ControllerBase
    {
        private readonly IMedicamentoInputPort _inputPort;

        public MedicamentoController(IMedicamentoInputPort inputPort)
        {
            _inputPort = inputPort;
        }

        [HttpGet]
        public IActionResult ObtenerTodos([FromQuery] string filtro = "")
        {
            var lista = string.IsNullOrEmpty(filtro)
                ? _inputPort.ObtenerTodos()
                : _inputPort.ObtenerTodos(filtro);

            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            Medicamento? medicamento = _inputPort.ObtenerPorId(id);

            if (medicamento == null)
                return NotFound(new { mensaje = "Medicamento no encontrado." });

            return Ok(medicamento);
        }

        [HttpPost]
        public IActionResult Crear([FromBody] MedicamentoCreateDto request)
        {
            var resultado = _inputPort.Crear(
                request.Nombre,
                request.Presentacion,
                request.IdClasificacion,
                request.Concentracion,
                request.Precio,
                request.Stock,
                request.IdUsuario
            );

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Medicamento creado correctamente." });
        }

        [HttpPut("{id:int}")]
        public IActionResult Actualizar(int id, [FromBody] MedicamentoCreateDto request)
        {
            var resultado = _inputPort.Actualizar(
                id,
                request.Nombre,
                request.Presentacion,
                request.IdClasificacion,
                request.Concentracion,
                request.Precio,
                request.Stock,
                request.IdUsuario
            );

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Medicamento actualizado correctamente." });
        }

        [HttpDelete("{id:int}")]
        public IActionResult Eliminar(int id, [FromQuery] int idUsuario)
        {
            var resultado = _inputPort.EliminarLogicamente(id, idUsuario);

            if (!resultado.IsSuccess)
                return BadRequest(new { mensaje = resultado.Error });

            return Ok(new { mensaje = "Medicamento eliminado correctamente." });
        }
    }
}
