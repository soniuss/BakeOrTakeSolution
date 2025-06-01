// En Persistence.ApiRest/Controllers/RecetasController.cs
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Necesario para acceder a los claims del token
using Microsoft.AspNetCore.Authorization; // Necesario para el atributo [Authorize]

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

        // Endpoint para obtener todas las recetas (puede ser público o autenticado)
        // GET /api/Recetas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Receta>>> GetRecetas()
        {
            return Ok(await _context.Recetas
                                    .Include(r => r.ClienteCreador) // Carga la información del cliente creador
                                    .ToListAsync());
        }

        // NUEVO: Endpoint para obtener una receta por ID
        // GET /api/Recetas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Receta>> GetReceta(int id)
        {
            var receta = await _context.Recetas
                                       .Include(r => r.ClienteCreador)
                                       .FirstOrDefaultAsync(r => r.id_receta == id);

            if (receta == null)
            {
                return NotFound();
            }

            return Ok(receta);
        }

        // NUEVO: Endpoint para crear una nueva receta
        // POST /api/Recetas
        [HttpPost]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden crear recetas
        public async Task<ActionResult<Receta>> CreateReceta([FromBody] Receta newReceta)
        {
            // Obtener el ID del cliente del token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteCreador))
            {
                return Unauthorized("No se pudo identificar al cliente creador.");
            }

            // Asignar el ID del cliente creador desde el token, no desde el body del request
            newReceta.id_cliente_creador = idClienteCreador;
            newReceta.fecha_registro = DateTime.UtcNow; // Asignar fecha de creación

            _context.Recetas.Add(newReceta);
            await _context.SaveChangesAsync();

            // Cargar el ClienteCreador para la respuesta si es necesario
            await _context.Entry(newReceta).Reference(r => r.ClienteCreador).LoadAsync();

            // Devolver 201 Created con la receta creada
            return CreatedAtAction(nameof(GetReceta), new { id = newReceta.id_receta }, newReceta);
        }

        // NUEVO: Endpoint para actualizar una receta existente
        // PUT /api/Recetas/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden actualizar recetas
        public async Task<IActionResult> UpdateReceta(int id, [FromBody] Receta updatedReceta)
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

        // NUEVO: Endpoint para obtener recetas por ID de creador (para "Mis Recetas")
        // GET /api/Recetas/ByCreator/{id_cliente_creador}
        [HttpGet("ByCreator/{id_cliente_creador}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden ver sus propias recetas
        public async Task<ActionResult<IEnumerable<Receta>>> GetRecetasByCreator(int id_cliente_creador)
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
            return Ok(recetas);
        }

        // Opcional: Endpoint para eliminar una receta
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