using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Helpers;
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
    public class ActoController : ControllerBase
    {
        private readonly IActoService _actoService;
        private readonly IPostulanteService _postulanteService;

        public ActoController(IActoService actoService, IPostulanteService postulanteService)
        {
            _actoService = actoService;
            _postulanteService = postulanteService;
        }

        // GET BY ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    Message = "El ID debe ser mayor a cero."
                });

            var acto = await _actoService.GetByIdAsync(id);

            if (acto == null)
                return NotFound(new
                {
                    Message = "El acto no existe."
                });

            return Ok(MapToResponse(acto));
        }

        // ADD
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add(
            [FromBody] ActoCreateDto request)
        {
            var validationError = ValidateActo(
                request.Numero,
                request.Descripcion,
                request.Inicio,
                request.Fin);

            if (validationError != null)
                return BadRequest(new { Message = validationError });

            var acto = new Acto
            {
                Numero = request.Numero.Trim(),
                Descripcion = request.Descripcion.Trim(),
                Inicio = request.Inicio!.Value,
                Fin = request.Fin!.Value,
                EsActivo = request.EsActivo,
                PermiteCargaCargos = request.PermiteCargaCargos
            };

            await _actoService.AddAsync(acto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = acto.Id },
                MapToResponse(acto));
        }

        // UPDATE
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] ActoUpdateDto request)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    Message = "El ID debe ser mayor a cero."
                });

            var validationError = ValidateActo(
                request.Numero,
                request.Descripcion,
                request.Inicio,
                request.Fin);

            if (validationError != null)
                return BadRequest(new { Message = validationError });

            var existingActo = await _actoService.GetByIdAsync(id);

            if (existingActo == null)
                return NotFound(new
                {
                    Message = "El acto no existe o fue eliminado."
                });

            var acto = new Acto
            {
                Id = id,
                Numero = request.Numero.Trim(),
                Descripcion = request.Descripcion.Trim(),
                Inicio = request.Inicio!.Value,
                Fin = request.Fin!.Value,
                EsActivo = request.EsActivo,
                PermiteCargaCargos = request.PermiteCargaCargos
            };

            try
            {
                await _actoService.UpdateAsync(acto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    Message = "El acto no existe o fue eliminado."
                });
            }

            var updatedActo = await _actoService.GetByIdAsync(id);

            return Ok(new
            {
                Message = "Acto actualizado correctamente.",
                Acto = updatedActo == null
                    ? null
                    : MapToResponse(updatedActo)
            });
        }

        // DELETE (SOFT DELETE)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    Message = "El ID debe ser mayor a cero."
                });

            var acto = await _actoService.GetByIdAsync(id);

            if (acto == null)
                return NotFound(new
                {
                    Message = "El acto no existe o ya fue eliminado."
                });

            try
            {
                await _actoService.DeleteAsync(acto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new
                {
                    Message = "El acto no existe o ya fue eliminado."
                });
            }

            return Ok(new
            {
                Message = "Acto eliminado correctamente."
            });
        }

        // GET PAGED
        [HttpGet("paged")]
        [Authorize(Roles = "admin,institucion")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] ActoQueryRequest request)
        {
            if (request.FechaDesde.HasValue &&
                request.FechaHasta.HasValue &&
                request.FechaDesde.Value.Date >
                    request.FechaHasta.Value.Date)
            {
                return BadRequest(new
                {
                    Message = "La fecha desde no puede ser posterior a la fecha hasta."
                });
            }

            if (request.Search?.Length > 100)
            {
                return BadRequest(new
                {
                    Message = "La búsqueda no puede superar los 100 caracteres."
                });
            }

            var result = await _actoService.GetPagedAsync(request);

            return Ok(new PagedResponse<ActoResponseDto>
            {
                Items = result.Items
                    .Select(MapToResponse)
                    .ToList(),

                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }

        // OBTENER ACTOS DISPONIBLES CON SUS CARGOS
        [HttpGet("disponibles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDisponibles()
        {
            var ahora = DateHelper.ArgentinaNow();

            int? postulanteId = null;

            // El endpoint sigue siendo público.
            // Solo buscamos el postulante si existe un usuario autenticado.
            if (User.Identity?.IsAuthenticated == true)
            {
                var postulante = await _postulanteService
                    .GetMiPostulanteAsync();

                if (postulante != null && !postulante.Eliminado)
                {
                    postulanteId = postulante.Id;
                }
            }

            var actos = await _actoService.GetDisponiblesAsync(
                ahora,
                postulanteId);

            return Ok(actos);
        }

        // VALIDACIONES COMUNES
        private static string? ValidateActo(
            string numero,
            string descripcion,
            DateTime? inicio,
            DateTime? fin)
        {
            if (string.IsNullOrWhiteSpace(numero))
                return "El número del acto es obligatorio.";

            if (string.IsNullOrWhiteSpace(descripcion))
                return "La descripción es obligatoria.";

            if (!inicio.HasValue || !fin.HasValue)
                return "Las fechas de inicio y fin son obligatorias.";

            if (inicio.Value == DateTime.MinValue ||
                fin.Value == DateTime.MinValue)
                return "Las fechas ingresadas no son válidas.";

            if (fin.Value <= inicio.Value)
                return "La fecha de finalización debe ser posterior a la de inicio.";

            return null;
        }

        // MAPEO A DTO
        private static ActoResponseDto MapToResponse(Acto acto)
        {
            var ahora = DateHelper.ArgentinaNow();

            return new ActoResponseDto
            {
                Id = acto.Id,
                Numero = acto.Numero,
                Descripcion = acto.Descripcion,
                Inicio = acto.Inicio,
                Fin = acto.Fin,
                EsActivo = acto.EsActivo,
                PermiteCargaCargos = acto.PermiteCargaCargos,
                PuedeCargarCargos = acto.PermiteCargaCargos && ahora < acto.Inicio.AddHours(-1),
                FechaCreacion = acto.FechaCreacion,
                FechaActualizacion = acto.FechaActualizacion,
                CreadoPor = acto.CreadoPor,
                ActualizadoPor = acto.ActualizadoPor
            };
        }
    }
}
