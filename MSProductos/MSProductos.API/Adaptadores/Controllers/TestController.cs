using Microsoft.AspNetCore.Mvc;

namespace MSProductos.API.Adaptadores.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("MSProductos funcionando correctamente");
        }
    }
}