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
    public class InstitucionController : ControllerBase
    {
        private readonly IInstitucionService _service;
        private readonly ICiudadService _ciudadService;        
        private readonly UserManager<AppUser> _userManager;

        public InstitucionController(
            IInstitucionService service,
            ICiudadService ciudadService,            
            UserManager<AppUser> userManager)
        {
            _service = service;
            _ciudadService = ciudadService;            
            _userManager = userManager;
        }

        // GET BY ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    Message = "El ID de institución es inválido."
                });

            var institucion = await _service.GetByIdAsync(id);

            if (institucion == null)
                return NotFound(new
                {
                    Message = "La institución no existe."
                });

            return Ok(MapToDto(institucion));
        }

        // GET PAGED
        [HttpGet("paged")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] InstitucionQueryRequest request)
        {
            if (request.Page <= 0 ||
                request.PageSize < 1 ||
                request.PageSize > 100)
            {
                return BadRequest(new
                {
                    Message = "Parámetros de paginación inválidos."
                });
            }

            if (request.CiudadId.HasValue &&
                request.CiudadId.Value <= 0)
            {
                return BadRequest(new
                {
                    Message = "El ID de ciudad es inválido."
                });
            }

            if (request.Search?.Length > 100)
            {
                return BadRequest(new
                {
                    Message = "La búsqueda no puede superar los 100 caracteres."
                });
            }

            var result = await _service.GetPagedAsync(request);

            return Ok(new PagedResponse<InstitucionResponseDto>
            {
                Items = result.Items
                    .Select(MapToDto)
                    .ToList(),
                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }

        // ADD
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Add(
            [FromBody] InstitucionCreateDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return BadRequest(new
                {
                    Message = "La descripción es obligatoria."
                });
            }

            var ciudadExiste = await _ciudadService.ExistsByIdAsync(request.CiudadId);

            if (!ciudadExiste)
            {
                return BadRequest(new
                {
                    Message = "La ciudad seleccionada no existe."
                });
            }

            var institucion = new Institucion
            {
                Descripcion = request.Descripcion.Trim(),
                CiudadId = request.CiudadId
            };

            var created = await _service.AddAsync(institucion);

            var result = await _service.GetByIdAsync(created.Id);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                MapToDto(result!));
        }

        // UPDATE
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] InstitucionUpdateDto request)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    Message = "El ID de institución es inválido."
                });

            if (string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return BadRequest(new
                {
                    Message = "La descripción es obligatoria."
                });
            }

            var existing = await _service.GetByIdAsync(id);

            if (existing == null)
                return NotFound(new
                {
                    Message = "La institución no existe o fue eliminada."
                });

            var ciudadExiste = await _ciudadService.ExistsByIdAsync(request.CiudadId);

            if (!ciudadExiste)
            {
                return BadRequest(new
                {
                    Message = "La ciudad seleccionada no existe."
                });
            }

            var institucion = new Institucion
            {
                Id = id,
                Descripcion = request.Descripcion.Trim(),
                CiudadId = request.CiudadId
            };

            var updated = await _service.UpdateAsync(institucion);

            if (!updated)
                return NotFound(new
                {
                    Message = "La institución no existe o fue eliminada."
                });

            var result = await _service.GetByIdAsync(id);

            return Ok(new
            {
                Message = "Institución actualizada correctamente.",
                Institucion = MapToDto(result!)
            });
        }

        // DELETE - SOFT DELETE
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    Message = "El ID de institución es inválido."
                });

            var existing = await _service.GetByIdAsync(id);

            if (existing == null)
                return NotFound(new
                {
                    Message = "La institución no existe o ya fue eliminada."
                });

            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound(new
                {
                    Message = "La institución no existe o ya fue eliminada."
                });

            return Ok(new
            {
                Message = "Institución eliminada correctamente."
            });
        }

        // ASIGNAR USUARIO
        [HttpPut("{id:int}/usuario")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AsignarUsuario(
            int id,
            [FromBody] AsignarUsuarioDto request)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    Message = "El ID de institución es inválido."
                });

            if (string.IsNullOrWhiteSpace(request.UsuarioId))
                return BadRequest(new
                {
                    Message = "Debe seleccionar un usuario."
                });

            var institucion = await _service.GetByIdAsync(id);

            if (institucion == null)
                return NotFound(new
                {
                    Message = "La institución no existe."
                });

            var usuario = await _userManager.FindByIdAsync(
                request.UsuarioId.Trim());

            if (usuario == null)
                return NotFound(new
                {
                    Message = "El usuario no existe."
                });

            var tieneRol = await _userManager.IsInRoleAsync(
                usuario, "institucion");

            if (!tieneRol)
                return BadRequest(new
                {
                    Message = "El usuario debe tener el rol institucion."
                });

            // Si ya está asignado a la misma institución
            if (institucion.UsuarioId == usuario.Id)
            {
                return Ok(new
                {
                    Message = "El usuario ya está asignado a esta institución."
                });
            }

            // La institución ya tiene otro usuario
            if (institucion.UsuarioId != null)
            {
                return Conflict(new
                {
                    Message = "La institución ya tiene un usuario asignado. Primero debe quitar la asignación actual."
                });
            }

            // Verificar asignación en otra institución,
            // incluyendo registros con Soft Delete.
            var asignadoAotra = await _service.UsuarioAsignadoAOtraInstitucionAsync(usuario.Id, id);

            if (asignadoAotra)
                return Conflict(new
                {
                    Message = "El usuario ya está asignado a otra institución."
                });

            try
            {
                await _service.AsignarUsuarioAsync(id, usuario.Id);
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    Message = "No se pudo completar la asignación. Verificá que el usuario no esté asignado a otra institución y que los registros sigan disponibles."
                });
            }
            catch (InvalidOperationException)
            {
                return Conflict(new
                {
                    Message = "La institución ya no está disponible para asignación."
                });
            }

            return Ok(new
            {
                Message = "Usuario asignado correctamente."
            });
        }

        // QUITAR USUARIO
        [HttpDelete("{id:int}/usuario")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> QuitarUsuario(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    Message = "El ID de institución es inválido."
                });

            var institucion = await _service.GetByIdAsync(id);

            if (institucion == null)
                return NotFound(new
                {
                    Message = "La institución no existe."
                });

            if (institucion.UsuarioId == null)
                return BadRequest(new
                {
                    Message = "La institución no tiene un usuario asignado."
                });

            var result = await _service.QuitarUsuarioAsync(id);

            if (!result)
                return NotFound(new
                {
                    Message = "La institución no existe."
                });

            return Ok(new
            {
                Message = "Usuario desasignado correctamente."
            });
        }

        // MI INSTITUCIÓN
        [HttpGet("mi-institucion")]
        [Authorize(Roles = "institucion")]
        public async Task<IActionResult> GetMiInstitucion()
        {
            var institucion =
                await _service.GetMiInstitucionAsync();

            if (institucion == null)
            {
                return NotFound(new
                {
                    Message = "No tenés una institución asignada."
                });
            }

            return Ok(new
            {
                institucion.Id,
                institucion.Descripcion,
                institucion.CiudadId,
                CiudadNombre = institucion.Ciudad?.Nombre ?? ""
            });
        }

        // LISTADO PARA SELECT / FILTROS
        [HttpGet("all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAll()
        {
            var instituciones =
                await _service.GetAllAsync();

            var result = instituciones
                .Select(i => new InstitucionSelectDto
                {
                    Id = i.Id,
                    Descripcion = i.Descripcion
                })
                .ToList();

            return Ok(result);
        }

        // MAPEO A DTO
        private static InstitucionResponseDto MapToDto(
            Institucion institucion)
        {
            return new InstitucionResponseDto
            {
                Id = institucion.Id,
                Descripcion = institucion.Descripcion,
                CiudadId = institucion.CiudadId,
                CiudadNombre = institucion.Ciudad?.Nombre ?? "",
                UsuarioId = institucion.UsuarioId,
                UsuarioEmail = institucion.Usuario?.Email,
                FechaCreacion = institucion.FechaCreacion,
                FechaActualizacion = institucion.FechaActualizacion,
                CreadoPor = institucion.CreadoPor,
                ActualizadoPor = institucion.ActualizadoPor
            };
        }
    }
}
