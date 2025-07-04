﻿using Domain.Model;
using Domain.Model.ApiRequests; 
using Domain.Model.ApiResponses; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

        // Endpoint para obtener las recetas favoritas de un cliente (ya existente)
        [HttpGet("ByClient/{id_cliente}")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<IEnumerable<RecetaResponse>>> GetFavoritosByClient(int id_cliente)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            if (idClienteActual != id_cliente)
            {
                return Forbid("No tienes permiso para ver los favoritos de otro usuario.");
            }

            var favoritos = await _context.Favoritos
                                          .Where(f => f.id_cliente == id_cliente)
                                          .Include(f => f.Receta)
                                              .ThenInclude(r => r.ClienteCreador)
                                          .Select(f => f.Receta)
                                          .ToListAsync();

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

        // Endpoint para verificar si una receta es favorita para un cliente (ya existente)
        [HttpGet("IsFavorite/{id_cliente}/{id_receta}")]
        [Authorize(Roles = "Cliente")]
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

        //Endpoint para añadir o eliminar una receta de favoritos (toggle)
        // POST /api/Favoritos/Toggle
        [HttpPost("Toggle")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> ToggleFavorito([FromBody] FavoritoToggleRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            var favoritoExistente = await _context.Favoritos
                                                .FirstOrDefaultAsync(f => f.id_cliente == idClienteActual && f.id_receta == request.IdReceta);

            if (favoritoExistente != null)
            {
                // Si existe, eliminarlo (desmarcar como favorito)
                _context.Favoritos.Remove(favoritoExistente);
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG: Favorito eliminado: Cliente {idClienteActual}, Receta {request.IdReceta}"); // Log de depuración
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
                Console.WriteLine($"DEBUG: Favorito añadido: Cliente {idClienteActual}, Receta {request.IdReceta}"); // Log de depuración
                return StatusCode(201); // 201 Created
            }
        }
    }
}
