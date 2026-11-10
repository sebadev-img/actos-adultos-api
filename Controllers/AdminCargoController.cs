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
    [Authorize(Roles = "admin")]
    public class AdminCargoController : ControllerBase
    {
        private readonly ICargoService _service;

        public AdminCargoController(
            ICargoService service)
        {
            _service = service;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] CargoQueryRequest request)
        {
            if (request.Search?.Length > 100)
            {
                return BadRequest(new
                {
                    Message =
                        "La búsqueda no puede superar los 100 caracteres."
                });
            }

            if (request.ActoId.HasValue &&
                request.ActoId.Value <= 0)
            {
                return BadRequest(new
                {
                    Message = "El ActoId no es válido."
                });
            }

            if (request.InstitucionId.HasValue &&
                request.InstitucionId.Value <= 0)
            {
                return BadRequest(new
                {
                    Message = "El InstitucionId no es válido."
                });
            }

            var result =
                await _service.GetPagedAdminAsync(request);

            return Ok(
                new PagedResponse<CargoResponseDto>
                {
                    Items = result.Items
                        .Select(MapToResponse)
                        .ToList(),

                    TotalItems = result.TotalItems,
                    Page = result.Page,
                    PageSize = result.PageSize
                });
        }

        private static CargoResponseDto MapToResponse(
            Cargo cargo)
        {
            return new CargoResponseDto
            {
                Id = cargo.Id,

                ActoId = cargo.ActoId,
                ActoDescripcion =
                    cargo.Acto?.Descripcion ?? "",

                InstitucionId =
                    cargo.InstitucionId,

                InstitucionNombre =
                    cargo.Institucion?.Descripcion ?? "",

                OrientacionId =
                    cargo.OrientacionId,

                OrientacionDescripcion =
                    cargo.Orientacion?.Descripcion ?? "",

                Turno = cargo.Turno,
                Descripcion = cargo.Descripcion,

                Codigo = cargo.Codigo,

                NumeroLLamado =
                    cargo.NumeroLLamado,

                Revista = cargo.Revista,

                Curso = cargo.Curso,
                Division = cargo.Division,

                PlazaSUNA = cargo.PlazaSUNA,

                CargaHoraria =
                    cargo.CargaHoraria,

                CodigoJunta =
                    cargo.CodigoJunta,

                Horarios = cargo.Horarios
                    .Where(h => !h.Eliminado)
                    .OrderBy(h => h.DiaSemana)
                    .ThenBy(h => h.HoraInicio)
                    .Select(h => new CargoHorarioDto
                    {
                        DiaSemana = h.DiaSemana,
                        HoraInicio = h.HoraInicio,
                        HoraFin = h.HoraFin
                    })
                    .ToList(),

                FechaCreacion =
                    cargo.FechaCreacion,

                FechaActualizacion =
                    cargo.FechaActualizacion
            };
        }
    }
}
