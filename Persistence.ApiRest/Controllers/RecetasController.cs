using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        // [Authorize] // Opcional: Descomentar si quieres que solo usuarios autenticados puedan ver recetas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Receta>>> GetRecetas()
        {
            // Incluir el ClienteCreador si lo necesitas en la respuesta
            return Ok(await _context.Recetas
                                    .Include(r => r.ClienteCreador) // Carga la informacion del cliente creador
                                    .ToListAsync());
        }

        // Puedes añadir mas endpoints aqui mas adelante:
        // - GET /api/Recetas/{id} para detalle de receta
        // - POST /api/Recetas para crear una nueva receta (requerira autenticacion)
        // - PUT /api/Recetas/{id} para actualizar una receta
        // - DELETE /api/Recetas/{id} para eliminar una receta
    }
}