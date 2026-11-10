using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ActosAdultosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrientacionController : ControllerBase
    {
        private readonly IOrientacionService _service;

        public OrientacionController(IOrientacionService service)
        {
            _service = service;
        }

        // AGREGAR ORIENTACIÓN
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add(
            [FromBody] OrientacionCreateDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return BadRequest(new
                {
                    Message = "La descripción es obligatoria."
                });
            }

            var orientacion = new Orientacion
            {
                Descripcion = request.Descripcion.Trim()
            };

            await _service.AddAsync(orientacion);

            return StatusCode(StatusCodes.Status201Created,
                new
                {
                    Message = "Orientación creada correctamente.",
                    Orientacion = MapToResponse(orientacion)
                });
        }

        // EDITAR ORIENTACIÓN
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] OrientacionUpdateDto request)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    Message = "El ID debe ser mayor a cero."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return BadRequest(new
                {
                    Message = "La descripción es obligatoria."
                });
            }

            var existing = await _service.GetByIdAsync(id);

            if (existing == null)
            {
                return NotFound(new
                {
                    Message = "La orientación no existe o fue eliminada."
                });
            }

            var orientacion = new Orientacion
            {
                Id = id,
                Descripcion = request.Descripcion.Trim()
            };

            try
            {
                await _service.UpdateAsync(orientacion);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    Message = "La orientación no existe o fue eliminada."
                });
            }

            var updated = await _service.GetByIdAsync(id);

            return Ok(new
            {
                Message = "Orientación actualizada correctamente.",
                Orientacion = updated == null
                    ? null
                    : MapToResponse(updated)
            });
        }

        // ELIMINAR ORIENTACIÓN (SOFT DELETE)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    Message = "El ID debe ser mayor a cero."
                });
            }

            var orientacion = await _service.GetByIdAsync(id);

            if (orientacion == null)
            {
                return NotFound(new
                {
                    Message = "La orientación no existe o ya fue eliminada."
                });
            }

            try
            {
                await _service.DeleteAsync(orientacion);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    Message = "La orientación no existe o ya fue eliminada."
                });
            }

            return Ok(new
            {
                Message = "Orientación eliminada correctamente."
            });
        }

        // LISTADO PAGINADO
        [HttpGet("paged")]
        [Authorize(Roles = "admin,institucion")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] OrientacionQueryRequest request)
        {
            if (request.Search?.Length > 100)
            {
                return BadRequest(new
                {
                    Message = "La búsqueda no puede superar los 100 caracteres."
                });
            }

            var result = await _service.GetPagedAsync(request);

            return Ok(new PagedResponse<OrientacionResponseDto>
            {
                Items = result.Items
                    .Select(MapToResponse)
                    .ToList(),

                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }

        // MAPEO A DTO
        private static OrientacionResponseDto MapToResponse(
            Orientacion orientacion)
        {
            return new OrientacionResponseDto
            {
                Id = orientacion.Id,
                Descripcion = orientacion.Descripcion,
                FechaCreacion = orientacion.FechaCreacion,
                CreadoPor = orientacion.CreadoPor,
                FechaActualizacion = orientacion.FechaActualizacion,
                ActualizadoPor = orientacion.ActualizadoPor
            };
        }
    }
}
