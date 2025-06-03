using Domain.Model; // Para Cliente, Receta, Favorito
using Domain.Model.ApiResponses; // Para RecetaResponse
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic; // Para List

namespace Persistence.ApiRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta base: /api/Favoritos
    public class FavoritosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoritosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // NUEVO: Endpoint para obtener las recetas favoritas de un cliente
        // GET /api/Favoritos/ByClient/{id_cliente}
        [HttpGet("ByClient/{id_cliente}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden ver sus favoritos
        public async Task<ActionResult<IEnumerable<RecetaResponse>>> GetFavoritosByClient(int id_cliente)
        {
            // Verificar que el ID del token coincide con el ID solicitado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            if (idClienteActual != id_cliente)
            {
                return Forbid("No tienes permiso para ver los favoritos de otro usuario.");
            }

            // Obtener los registros de Favorito e incluir la Receta y su ClienteCreador
            var favoritos = await _context.Favoritos
                                          .Where(f => f.id_cliente == id_cliente)
                                          .Include(f => f.Receta)
                                              .ThenInclude(r => r.ClienteCreador) // Incluir el creador de la receta
                                          .Select(f => f.Receta) // Seleccionar solo la Receta del Favorito
                                          .ToListAsync();

            // Mapear las Recetas obtenidas a RecetaResponse DTOs
            var recetaResponses = favoritos.Select(r => new RecetaResponse
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

        // Opcional: Endpoint para añadir/eliminar favoritos
        // [HttpPost("toggle")]
        // [Authorize(Roles = "Cliente")]
        // public async Task<IActionResult> ToggleFavorito([FromBody] FavoritoRequest request) { ... }
    }
}
