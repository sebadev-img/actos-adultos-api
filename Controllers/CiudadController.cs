using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ActosAdultosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CiudadController : ControllerBase
    {
        private readonly ICiudadService _service;

        public CiudadController(ICiudadService service)
        {
            _service = service;
        }

        [HttpGet("GetCiudades")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCiudades()
        {
            var ciudades = await _service.GetCiudades();

            return Ok(ciudades);
        }
    }
}
