using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Helpers;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ActosAdultosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "institucion")]
    public class CargoController : ControllerBase
    {
        private readonly ICargoService _service;
        private readonly IInstitucionService _institucionService;
        private readonly IActoService _actoService;
        private readonly IOrientacionService _orientacionService;
        private readonly IJuntaApiService _juntaApiService;

        public CargoController(
            ICargoService service,
            IInstitucionService institucionService,
            IActoService actoService,
            IOrientacionService orientacionService,
            IJuntaApiService juntaApiService)
        {
            _service = service;
            _institucionService = institucionService;
            _actoService = actoService;
            _orientacionService = orientacionService;
            _juntaApiService = juntaApiService;
        }

        // AGREGAR CARGO
        [HttpPost]
        public async Task<IActionResult> Add(
            [FromBody] CargoCreateDto request)
        {
            var institucion = await _institucionService
                .GetMiInstitucionAsync();

            if (institucion == null || institucion.Eliminado)
            {
                return Forbid();
            }

            var error = await ValidateCargoAsync(request);

            if (error != null)
            {
                return BadRequest(new { Message = error });
            }

            var cargo = MapToEntity(
                request, institucion.Id);

            await _service.AddAsync(cargo);

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    Message = "Cargo creado correctamente.",
                    CargoId = cargo.Id
                });
        }

        // EDITAR CARGO
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CargoUpdateDto request)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    Message = "El ID debe ser mayor a cero."
                });
            }

            var institucion = await _institucionService
                .GetMiInstitucionAsync();

            if (institucion == null || institucion.Eliminado)
            {
                return Forbid();
            }

            // La búsqueda también valida la pertenencia.
            var existing = await _service.GetByIdAsync(
                id, institucion.Id);

            if (existing == null)
            {
                return NotFound(new
                {
                    Message = "El cargo no existe o no pertenece a tu institución."
                });
            }

            // No permitir mover el cargo a otro acto.
            // Su acto debe permanecer fijo después de crearlo.
            if (request.ActoId != existing.ActoId)
            {
                return BadRequest(new
                {
                    Message = "No se puede cambiar el acto de un cargo existente."
                });
            }

            var error = await ValidateCargoAsync(request);

            if (error != null)
            {
                return BadRequest(new { Message = error });
            }

            var cargo = MapToEntity(
                request, institucion.Id);

            cargo.Id = id;

            var updated = await _service.UpdateAsync(
                cargo, institucion.Id);

            if (!updated)
            {
                return NotFound(new
                {
                    Message = "El cargo no existe o no pertenece a tu institución."
                });
            }

            var result = await _service.GetByIdAsync(
                id, institucion.Id);

            return Ok(new
            {
                Message = "Cargo actualizado correctamente.",
                Cargo = result == null
                    ? null
                    : MapToResponse(result)
            });
        }

        // LISTADO PAGINADO
        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] CargoQueryRequest request)
        {
            if (request.Search?.Length > 100)
            {
                return BadRequest(new
                {
                    Message = "La búsqueda no puede superar los 100 caracteres."
                });
            }

            if (request.ActoId.HasValue &&
                request.ActoId.Value <= 0)
            {
                return BadRequest(new
                {
                    Message = "El ActoId debe ser mayor a cero."
                });
            }

            if (request.OrientacionId.HasValue &&
                request.OrientacionId.Value <= 0)
            {
                return BadRequest(new
                {
                    Message = "El OrientacionId debe ser mayor a cero."
                });
            }

            if (request.Turno.HasValue &&
                !Enum.IsDefined(request.Turno.Value))
            {
                return BadRequest(new
                {
                    Message = "El turno seleccionado no es válido."
                });
            }

            var institucion = await _institucionService
                .GetMiInstitucionAsync();

            if (institucion == null || institucion.Eliminado)
            {
                return Forbid();
            }

            var result = await _service.GetPagedAsync(
                request, institucion.Id);

            return Ok(new PagedResponse<CargoResponseDto>
            {
                Items = result.Items
                    .Select(MapToResponse)
                    .ToList(),

                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }


        [HttpGet("getCodigoJunta")]
        public async Task<IActionResult> GetCodigoJunta(
            CancellationToken cancellationToken)
        {
            // Verificar que el usuario autenticado tenga institución
            var institucion = await _institucionService
                .GetMiInstitucionAsync();

            if (institucion == null || institucion.Eliminado)
            {
                return Forbid();
            }

            try
            {
                var codigos = await _juntaApiService
                    .GetCodigosJuntaAsync(cancellationToken);

                return Ok(codigos);
            }
            catch (OperationCanceledException)
                when (!cancellationToken.IsCancellationRequested)
            {
                return StatusCode(504, new
                {
                    Message = "La API de Junta no respondió a tiempo."
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new
                {
                    Message = "No se pudo consultar la API de Junta."
                });
            }
            catch (System.Text.Json.JsonException)
            {
                return StatusCode(502, new
                {
                    Message = "La API de Junta devolvió un formato inválido."
                });
            }
            catch (JsonSerializationException)
            {
                return StatusCode(502, new
                {
                    Message = "La API de Junta devolvió datos inválidos."
                });
            }
            catch (JsonReaderException)
            {
                return StatusCode(502, new
                {
                    Message = "No se pudo interpretar la respuesta de Junta."
                });
            }
        }


        // VALIDAR DATOS Y RELACIONES
        private async Task<string?> ValidateCargoAsync(
            CargoCreateDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Descripcion))
                return "La descripción del cargo es obligatoria.";

            if (string.IsNullOrWhiteSpace(request.Division))
                return "La división es obligatoria.";

            if (string.IsNullOrWhiteSpace(request.PlazaSUNA))
                return "La plaza SUNA es obligatoria.";

            if (string.IsNullOrWhiteSpace(request.CargaHoraria))
                return "La carga horaria es obligatoria.";

            if (string.IsNullOrWhiteSpace(request.CodigoJunta))
                return "El código de Junta es obligatorio.";

            if (!Enum.IsDefined(request.Turno))
                return "El turno seleccionado no es válido.";

            if (!Enum.IsDefined(request.Revista))
                return "La revista seleccionada no es válida.";

            if (request.Codigo < 0 ||
                request.NumeroLLamado < 0 ||
                request.Curso < 0)
            {
                return "Los valores numéricos no pueden ser negativos.";
            }

            var acto = await _actoService.GetByIdAsync(request.ActoId);

            if (acto == null)
                return "El acto seleccionado no existe.";

            if (!acto.PermiteCargaCargos)
                return "El acto no está habilitado para la carga de cargos.";

            // Fecha y hora actual de Argentina
            var ahora = DateHelper.ArgentinaNow();

            // Fecha límite para cargar cargos
            var fechaLimite = acto.Inicio.AddHours(-1);

            // No permitir cargar cargos desde una hora antes del inicio
            if (ahora >= fechaLimite)
            {
                return "El plazo para cargar cargos ha finalizado. " +
                       "Solo se permite cargar cargos hasta una hora antes " +
                       "del inicio del acto.";
            }

            var orientacion = await _orientacionService
                .GetByIdAsync(request.OrientacionId);

            if (orientacion == null)
                return "La orientación seleccionada no existe.";

            return ValidateHorarios(request.Horarios);
        }

        // VALIDACIÓN DE HORARIOS
        private static string? ValidateHorarios(
            List<CargoHorarioDto>? horarios)
        {
            if (horarios == null || horarios.Count == 0)
                return "Debe agregar al menos un horario.";

            foreach (var horario in horarios)
            {
                if (!Enum.IsDefined(horario.DiaSemana))
                    return "El día de la semana no es válido.";

                if (horario.HoraInicio >= horario.HoraFin)
                {
                    return "La hora de inicio debe ser anterior a la hora de fin.";
                }
            }

            // Verificar superposiciones dentro de cada día.
            foreach (var grupo in horarios.GroupBy(h => h.DiaSemana))
            {
                var ordenados = grupo
                    .OrderBy(h => h.HoraInicio)
                    .ToList();

                for (int i = 1; i < ordenados.Count; i++)
                {
                    if (ordenados[i].HoraInicio <
                        ordenados[i - 1].HoraFin)
                    {
                        return $"Existen horarios superpuestos el día {grupo.Key}.";
                    }
                }
            }

            return null;
        }

        // MAPEO DE DTO A ENTIDAD
        private static Cargo MapToEntity(
            CargoCreateDto request,
            int institucionId)
        {
            return new Cargo
            {
                ActoId = request.ActoId,
                InstitucionId = institucionId,
                OrientacionId = request.OrientacionId,

                Turno = request.Turno,
                Descripcion = request.Descripcion.Trim(),
                Codigo = request.Codigo,
                NumeroLLamado = request.NumeroLLamado,
                Revista = request.Revista,
                Curso = request.Curso,
                Division = request.Division.Trim(),
                PlazaSUNA = request.PlazaSUNA.Trim(),
                CargaHoraria = request.CargaHoraria.Trim(),
                CodigoJunta = request.CodigoJunta.Trim(),

                Horarios = request.Horarios.Select(h =>
                    new CargoHorario
                    {
                        DiaSemana = h.DiaSemana,
                        HoraInicio = h.HoraInicio,
                        HoraFin = h.HoraFin
                    }).ToList()
            };
        }

        // MAPEO DE ENTIDAD A DTO
        private static CargoResponseDto MapToResponse(
            Cargo cargo)
        {
            return new CargoResponseDto
            {
                Id = cargo.Id,

                ActoId = cargo.ActoId,
                ActoDescripcion = cargo.Acto?.Descripcion ?? "",

                InstitucionId = cargo.InstitucionId,
                InstitucionNombre =
                    cargo.Institucion?.Descripcion ?? "",

                OrientacionId = cargo.OrientacionId,
                OrientacionDescripcion =
                    cargo.Orientacion?.Descripcion ?? "",

                Turno = cargo.Turno,
                Descripcion = cargo.Descripcion,
                Codigo = cargo.Codigo,
                NumeroLLamado = cargo.NumeroLLamado,
                Revista = cargo.Revista,
                Curso = cargo.Curso,
                Division = cargo.Division,
                PlazaSUNA = cargo.PlazaSUNA,
                CargaHoraria = cargo.CargaHoraria,
                CodigoJunta = cargo.CodigoJunta,

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

                FechaCreacion = cargo.FechaCreacion,
                FechaActualizacion = cargo.FechaActualizacion
            };
        }
    }
}
