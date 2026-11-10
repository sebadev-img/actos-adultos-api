using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Models;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActosAdultosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostulanteController : ControllerBase
    {
        private readonly IPostulanteService _service;
        private readonly ICiudadService _ciudadService;        
        private readonly UserManager<AppUser> _userManager;

        public PostulanteController(
            IPostulanteService service,
            ICiudadService ciudadService,            
            UserManager<AppUser> userManager)
        {
            _service = service;
            _ciudadService = ciudadService;            
            _userManager = userManager;
        }

        // ADD OR UPDATE
        [HttpPost("AddOrUpdate")]
        [Authorize(Roles = "admin,docente")]
        public async Task<IActionResult> AddOrUpdate(
            [FromBody] PostulanteAddOrUpdateDto request)
        {
            // Obtener usuario autenticado
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    Message = "No se pudo identificar al usuario."
                });
            }

            // Validar campos de texto
            if (string.IsNullOrWhiteSpace(request.Nombres))
            {
                return BadRequest(new
                {
                    Message = "Los nombres son obligatorios."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Apellidos))
            {
                return BadRequest(new
                {
                    Message = "Los apellidos son obligatorios."
                });
            }

            // Validar ciudad
            var ciudadExiste = await _ciudadService.ExistsByIdAsync(request.CiudadId);

            if (!ciudadExiste)
            {
                return BadRequest(new
                {
                    Message = "La ciudad seleccionada no existe."
                });
            }

            // Buscar postulante del usuario autenticado
            var existing = await _service.GetMiPostulanteAsync();

            // Buscar el postulante asociado al DNI
            var postulanteConDni = await _service.GetByDniAsync(
                request.Dni!.Value);

            // Verificar que el DNI no pertenezca a otro postulante
            if (postulanteConDni != null &&
                postulanteConDni.Id != existing?.Id)
            {
                return Conflict(new
                {
                    Message = "Ya existe un postulante registrado con ese DNI.",
                    Code = "DNI_ALREADY_EXISTS"
                });
            }

            // Valores compartidos entre creación y edición
            var postulante = new Postulante
            {
                Dni = request.Dni.Value,
                Nombres = request.Nombres.Trim(),
                Apellidos = request.Apellidos.Trim(),
                Email = request.Email?.Trim(),
                Telefono = request.Telefono?.Trim(),
                Direccion = request.Direccion?.Trim(),
                Altura = request.Altura?.Trim(),
                Depto = request.Depto?.Trim(),
                CiudadId = request.CiudadId
            };

            // CREAR
            if (existing == null)
            {
                postulante.UsuarioId = userId;

                try
                {
                    var created = await _service.AddAsync(postulante);

                    var result = await _service.GetByIdAsync(
                        created.Id);

                    return StatusCode(
                        StatusCodes.Status201Created,
                        new
                        {
                            Message = "Postulante registrado correctamente.",
                            Postulante = MapToDto(result!)
                        });
                }
                catch (DbUpdateException)
                {
                    return Conflict(new
                    {
                        Message = "No se pudo registrar el postulante. Verificá que el DNI y el usuario no estén asociados a otro registro."
                    });
                }
            }

            // ACTUALIZAR
            postulante.Id = existing.Id;

            try
            {
                var updated = await _service.UpdateAsync(postulante);

                if (!updated)
                {
                    return NotFound(new
                    {
                        Message = "El postulante no existe."
                    });
                }
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    Message = "No se pudo actualizar el postulante debido a un conflicto de datos."
                });
            }

            var updatedPostulante = await _service.GetByIdAsync(
                existing.Id);

            return Ok(new
            {
                Message = "Postulante actualizado correctamente.",
                Postulante = MapToDto(updatedPostulante!)
            });
        }

        // GET BY ID - ADMIN
        [HttpGet("GetById/{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    Message = "El ID debe ser mayor a cero."
                });
            }

            var postulante = await _service.GetByIdAsync(id);

            if (postulante == null)
            {
                return NotFound(new
                {
                    Message = "El postulante no existe."
                });
            }

            return Ok(MapToDto(postulante));
        }

        // GET MI POSTULANTE
        [HttpGet("GetMiPostulante")]
        [Authorize(Roles = "admin,docente")]
        public async Task<IActionResult> GetMiPostulante()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    Message = "No se pudo identificar al usuario."
                });
            }

            var postulante = await _service.GetMiPostulanteAsync();

            if (postulante == null)
            {
                return NotFound(new
                {
                    Message = "No tenés datos de postulante registrados."
                });
            }

            return Ok(MapToDto(postulante));
        }

        // GET POSTULANTES PAGINADOS - ADMIN
        [HttpGet("GetPaged")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] PostulanteQueryRequest request)
        {
            var result = await _service.GetPagedAsync(request);

            var response = new PagedResponse<PostulanteResponseDto>
            {
                Items = result.Items
                    .Select(MapToDto)
                    .ToList(),

                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize
            };

            return Ok(response);
        }

        // MAPEO ENTITY -> DTO
        private static PostulanteResponseDto MapToDto(
            ActosAdultosAPI.Models.Postulante postulante)
        {
            return new PostulanteResponseDto
            {
                Id = postulante.Id,
                Dni = postulante.Dni,

                Nombres = postulante.Nombres,
                Apellidos = postulante.Apellidos,
                NombreCompleto =
                    $"{postulante.Apellidos}, {postulante.Nombres}",

                Email = postulante.Email,
                Telefono = postulante.Telefono,

                Direccion = postulante.Direccion,
                Altura = postulante.Altura,
                Depto = postulante.Depto,

                CiudadId = postulante.CiudadId,
                CiudadNombre = postulante.Ciudad?.Nombre ?? "",

                UsuarioId = postulante.UsuarioId,

                FechaCreacion = postulante.FechaCreacion,
                FechaActualizacion = postulante.FechaActualizacion,

                CreadoPor = postulante.CreadoPor,
                ActualizadoPor = postulante.ActualizadoPor
            };
        }
    }
}
