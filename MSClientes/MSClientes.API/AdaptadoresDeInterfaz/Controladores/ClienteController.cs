using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSClientes.API.CasosDeUso.PuertosEntrada;
using MSClientes.API.Entidades;

namespace MSClientes.API.AdaptadoresDeInterfaz.Controladores
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize(Roles = "Admin,Bioquimico")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteInputPort clienteInputPort;

        public ClienteController(IClienteInputPort clienteInputPort)
        {
            this.clienteInputPort = clienteInputPort;
        }

        [HttpGet]
        public IActionResult ObtenerTodos([FromQuery] string filtro = "")
        {
            return Ok(clienteInputPort.ObtenerTodos(filtro));
        }

        [HttpGet("{id:int}")]
        public IActionResult ObtenerPorId(int id)
        {
            Cliente? cliente = clienteInputPort.ObtenerPorId(id);
            return cliente == null ? NotFound(new { mensaje = "Cliente no encontrado." }) : Ok(cliente);
        }

        [HttpPost]
        public IActionResult Crear([FromBody] Cliente cliente)
        {
            var resultado = clienteInputPort.Crear(cliente);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }

        [HttpPut("{id:int}")]
        public IActionResult Actualizar(int id, [FromBody] Cliente cliente)
        {
            var resultado = clienteInputPort.Actualizar(id, cliente);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }

        [HttpDelete("{id:int}")]
        public IActionResult Eliminar(int id, [FromQuery] int idUsuario)
        {
            var resultado = clienteInputPort.Eliminar(id, idUsuario);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return Ok(new { mensaje = resultado.Mensaje });
        }
    }
}
