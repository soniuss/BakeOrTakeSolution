using Domain.Model; // Para Cliente, Receta, Favorito
using Domain.Model.ApiResponses; // Para RecetaResponse
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Model.ApiRequests; // Para List

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

        // ¡NUEVO! Endpoint para verificar si una receta es favorita para un cliente
        // GET /api/Favoritos/IsFavorite/{id_cliente}/{id_receta}
        [HttpGet("IsFavorite/{id_cliente}/{id_receta}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados
        public async Task<ActionResult<bool>> IsFavorite(int id_cliente, int id_receta)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            if (idClienteActual != id_cliente)
            {
                return Forbid("No tienes permiso para comprobar favoritos de otro usuario.");
            }

            bool isFavorite = await _context.Favoritos
                                            .AnyAsync(f => f.id_cliente == id_cliente && f.id_receta == id_receta);
            return Ok(isFavorite);
        }

        // ¡NUEVO! Endpoint para añadir o eliminar una receta de favoritos (toggle)
        // POST /api/Favoritos/Toggle
        [HttpPost("Toggle")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados
        public async Task<IActionResult> ToggleFavorito([FromBody] FavoritoToggleRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            // Verificar que el IdCliente en el request (si se enviara) coincide con el del token.
            // En este caso, el request solo tiene IdReceta, el cliente ID se toma del token.

            var favoritoExistente = await _context.Favoritos
                                                .FirstOrDefaultAsync(f => f.id_cliente == idClienteActual && f.id_receta == request.IdReceta);

            if (favoritoExistente != null)
            {
                // Si existe, eliminarlo (desmarcar como favorito)
                _context.Favoritos.Remove(favoritoExistente);
                await _context.SaveChangesAsync();
                return NoContent(); // 204 No Content
            }
            else
            {
                // Si no existe, añadirlo (marcar como favorito)
                var nuevaFavorito = new Favorito
                {
                    id_cliente = idClienteActual,
                    id_receta = request.IdReceta
                };
                _context.Favoritos.Add(nuevaFavorito);
                await _context.SaveChangesAsync();
                return StatusCode(201); // 201 Created (o 204 No Content si prefieres no devolver nada)
            }
        }
    }
}
