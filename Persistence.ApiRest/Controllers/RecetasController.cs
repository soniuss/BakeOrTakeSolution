using Domain.Model; 
using Domain.Model.ApiResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization; 

namespace Persistence.ApiRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta base: /api/Recetas
    public class RecetasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecetasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint para obtener todas las recetas
        // GET /api/Recetas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecetaResponse>>> GetRecetas() 
        {
            var recetas = await _context.Recetas
                                        .Include(r => r.ClienteCreador) // Incluimos el creador para el mapeo a DTO
                                        .ToListAsync();

            // Mapear de Receta (dominio) a RecetaResponse (DTO)
            var recetaResponses = recetas.Select(r => new RecetaResponse
            {
                IdReceta = r.id_receta,
                Nombre = r.nombre,
                Descripcion = r.descripcion,
                Ingredientes = r.ingredientes,
                Pasos = r.pasos,
                ImagenUrl = r.imagenUrl,
                FechaRegistro = r.fecha_registro,
                IdClienteCreador = r.id_cliente_creador,
                ClienteCreadorNombre = r.ClienteCreador != null ? r.ClienteCreador.nombre : "Desconocido",
                ClienteCreadorEmail = r.ClienteCreador != null ? r.ClienteCreador.email : "Desconocido"
            }).ToList();

            return Ok(recetaResponses);
        }

        // Endpoint para obtener una receta por ID (PÚBLICO)
        // GET /api/Recetas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RecetaResponse>> GetReceta(int id) 
        {
            var receta = await _context.Recetas
                                       .Include(r => r.ClienteCreador)
                                       .FirstOrDefaultAsync(r => r.id_receta == id);

            if (receta == null)
            {
                return NotFound();
            }

            // Mapear a RecetaResponse
            var recetaResponse = new RecetaResponse
            {
                IdReceta = receta.id_receta,
                Nombre = receta.nombre,
                Descripcion = receta.descripcion,
                Ingredientes = receta.ingredientes,
                Pasos = receta.pasos,
                ImagenUrl = receta.imagenUrl,
                FechaRegistro = receta.fecha_registro,
                IdClienteCreador = receta.id_cliente_creador,
                ClienteCreadorNombre = receta.ClienteCreador != null ? receta.ClienteCreador.nombre : "Desconocido",
                ClienteCreadorEmail = receta.ClienteCreador != null ? receta.ClienteCreador.email : "Desconocido"
            };

            return Ok(recetaResponse);
        }

        // Endpoint para crear una nueva receta (PROTEGIDO)
        // POST /api/Recetas
        [HttpPost]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden crear recetas
        public async Task<ActionResult<RecetaResponse>> CreateReceta([FromBody] Receta newReceta) 
        {
            // Obtener el ID del cliente del token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteCreador))
            {
                return Unauthorized("No se pudo identificar al cliente creador.");
            }

            // Asignar el ID del cliente creador desde el token, no desde el body del request
            newReceta.id_cliente_creador = idClienteCreador;
            newReceta.fecha_registro = DateTime.UtcNow;

            _context.Recetas.Add(newReceta);
            await _context.SaveChangesAsync();

            // Cargar el ClienteCreador para el mapeo al DTO de respuesta
            await _context.Entry(newReceta).Reference(r => r.ClienteCreador).LoadAsync();

            var recetaResponse = new RecetaResponse
            {
                IdReceta = newReceta.id_receta,
                Nombre = newReceta.nombre,
                Descripcion = newReceta.descripcion,
                Ingredientes = newReceta.ingredientes,
                Pasos = newReceta.pasos,
                ImagenUrl = newReceta.imagenUrl,
                FechaRegistro = newReceta.fecha_registro,
                IdClienteCreador = newReceta.id_cliente_creador,
                ClienteCreadorNombre = newReceta.ClienteCreador != null ? newReceta.ClienteCreador.nombre : "Desconocido",
                ClienteCreadorEmail = newReceta.ClienteCreador != null ? newReceta.ClienteCreador.email : "Desconocido"
            };

            return CreatedAtAction(nameof(GetReceta), new { id = recetaResponse.IdReceta }, recetaResponse);
        }

        // Endpoint para actualizar una receta existente (PROTEGIDO)
        // PUT /api/Recetas/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden actualizar recetas
        public async Task<IActionResult> UpdateReceta(int id, [FromBody] Receta updatedReceta) // Recibe Receta, devuelve IActionResult
        {
            if (id != updatedReceta.id_receta)
            {
                return BadRequest("El ID de la ruta no coincide con el ID de la receta en el cuerpo.");
            }

            // Obtener el ID del cliente del token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            // Encontrar la receta existente en la base de datos
            var existingReceta = await _context.Recetas.FindAsync(id);
            if (existingReceta == null)
            {
                return NotFound();
            }

            // **VERIFICACIÓN DE AUTORIZACIÓN: Solo el creador puede editar la receta**
            if (existingReceta.id_cliente_creador != idClienteActual)
            {
                return Forbid("No tienes permiso para editar esta receta. Solo el creador puede hacerlo.");
            }

            // Actualizar solo los campos permitidos
            existingReceta.nombre = updatedReceta.nombre;
            existingReceta.descripcion = updatedReceta.descripcion;
            existingReceta.ingredientes = updatedReceta.ingredientes;
            existingReceta.pasos = updatedReceta.pasos;
            existingReceta.imagenUrl = updatedReceta.imagenUrl;
            // No actualizar id_cliente_creador ni fecha_registro aquí

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Recetas.Any(e => e.id_receta == id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Relanzar si es otro tipo de error de concurrencia
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar receta: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al actualizar la receta.");
            }

            return NoContent(); // 204 No Content indica éxito sin devolver un cuerpo
        }

        // Endpoint para obtener recetas por ID de creador (PROTEGIDO)
        // GET /api/Recetas/ByCreator/{id_cliente_creador}
        [HttpGet("ByCreator/{id_cliente_creador}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden ver sus propias recetas
        public async Task<ActionResult<IEnumerable<RecetaResponse>>> GetRecetasByCreator(int id_cliente_creador)
        {
            // Opcional: Verificar que el ID del token coincide con el ID solicitado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            if (idClienteActual != id_cliente_creador)
            {
                return Forbid("No tienes permiso para ver las recetas de otro usuario.");
            }

            var recetas = await _context.Recetas
                                        .Include(r => r.ClienteCreador)
                                        .Where(r => r.id_cliente_creador == id_cliente_creador)
                                        .ToListAsync();

            // Mapear a RecetaResponse
            var recetaResponses = recetas.Select(r => new RecetaResponse
            {
                IdReceta = r.id_receta,
                Nombre = r.nombre,
                Descripcion = r.descripcion,
                Ingredientes = r.ingredientes,
                Pasos = r.pasos,
                ImagenUrl = r.imagenUrl,
                FechaRegistro = r.fecha_registro,
                IdClienteCreador = r.id_cliente_creador,
                ClienteCreadorNombre = r.ClienteCreador != null ? r.ClienteCreador.nombre : "Desconocido",
                ClienteCreadorEmail = r.ClienteCreador != null ? r.ClienteCreador.email : "Desconocido"
            }).ToList();

            return Ok(recetaResponses);
        }

        // Endpoint para obtener recetas por ID de empresa (PROTEGIDO)
        // GET /api/Recetas/ByCompany/{id_empresa}
        [HttpGet("ByCompany/{id_empresa}")]
        [Authorize(Roles = "Empresa")] // Solo empresas autenticadas pueden ver sus recetas/ofertas
        public async Task<ActionResult<IEnumerable<RecetaResponse>>> GetRecetasByCompany(int id_empresa) 
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idEmpresaActual))
            {
                return Unauthorized("No se pudo identificar a la empresa actual.");
            }

            if (idEmpresaActual != id_empresa)
            {
                return Forbid("No tienes permiso para ver las recetas de otra empresa.");
            }

            var recetasDeEmpresa = await _context.PedidosOfertas
                                                .Where(po => po.id_empresa == id_empresa)
                                                .Select(po => po.Receta)
                                                .Distinct()
                                                .Include(r => r.ClienteCreador)
                                                .ToListAsync();

            var recetaResponses = recetasDeEmpresa.Select(r => new RecetaResponse
            {
                IdReceta = r.id_receta,
                Nombre = r.nombre,
                Descripcion = r.descripcion,
                Ingredientes = r.ingredientes,
                Pasos = r.pasos,
                ImagenUrl = r.imagenUrl,
                FechaRegistro = r.fecha_registro,
                IdClienteCreador = r.id_cliente_creador,
                ClienteCreadorNombre = r.ClienteCreador != null ? r.ClienteCreador.nombre : "Desconocido",
                ClienteCreadorEmail = r.ClienteCreador != null ? r.ClienteCreador.email : "Desconocido"
            }).ToList();

            return Ok(recetaResponses);
        }


        // Endpoint para eliminar una receta (PROTEGIDO)
        // DELETE /api/Recetas/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden eliminar recetas
        public async Task<IActionResult> DeleteReceta(int id)
        {
            var receta = await _context.Recetas.FindAsync(id);
            if (receta == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            if (receta.id_cliente_creador != idClienteActual)
            {
                return Forbid("No tienes permiso para eliminar esta receta. Solo el creador puede hacerlo.");
            }

            _context.Recetas.Remove(receta);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
    }
}
