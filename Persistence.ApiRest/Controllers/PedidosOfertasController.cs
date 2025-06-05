using Domain.Model; // Para las entidades de dominio (PedidoOferta, Empresa, Receta, Cliente)
using Domain.Model.ApiRequests; // Para OfertaRequest, PedidoRequest
using Domain.Model.ApiResponses; // Para PedidoOfertaResponse
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Para Include, Where, ToListAsync, FindAsync
using System.Linq; // Para Select, Any
using System.Security.Claims; // Para acceder a los claims del token (UserId, Role)
using Microsoft.AspNetCore.Authorization; // Para el atributo [Authorize]
using System.Threading.Tasks; // Para Task
using System.Collections.Generic; // Para IEnumerable, List
using System; // Para DateTime, Console.WriteLine

namespace Persistence.ApiRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta base: /api/PedidosOfertas
    public class PedidosOfertasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PedidosOfertasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint para obtener pedidos realizados por un cliente específico
        // GET /api/PedidosOfertas/ByClient/{id_cliente}
        [HttpGet("ByClient/{id_cliente}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden ver sus pedidos
        public async Task<ActionResult<IEnumerable<PedidoOfertaResponse>>> GetClientOrders(int id_cliente)
        {
            // Verificar que el ID del token coincide con el ID solicitado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            if (idClienteActual != id_cliente)
            {
                return Forbid("No tienes permiso para ver los pedidos de otro usuario.");
            }

            // Obtener los pedidos del cliente, incluyendo la Empresa y la Receta relacionada
            var pedidos = await _context.PedidosOfertas
                                        .Where(po => po.id_cliente == id_cliente) // Filtra por el cliente que REALIZÓ el pedido
                                        .Include(po => po.Empresa) // Para obtener el nombre de la empresa
                                        .Include(po => po.Receta)  // Para obtener el nombre de la receta
                                        .ToListAsync();

            // Mapear la entidad de dominio a PedidoOfertaResponse DTO
            var pedidoResponses = pedidos.Select(po => new PedidoOfertaResponse
            {
                IdPedidoOferta = po.id_pedido_oferta,
                Precio = po.precio,
                Disponibilidad = po.disponibilidad,
                DescripcionOferta = po.descripcionOferta,
                IdEmpresa = po.id_empresa,
                EmpresaNombreNegocio = po.Empresa != null ? po.Empresa.nombre_negocio : "Desconocido",
                EmpresaUbicacion = po.Empresa != null ? po.Empresa.ubicacion : "Desconocido",
                IdReceta = po.id_receta,
                RecetaNombre = po.Receta != null ? po.Receta.nombre : "Desconocido",
                RecetaImagenUrl = po.Receta != null ? po.Receta.imagenUrl : null,
                IdClienteRealiza = po.id_cliente,
                ClienteRealizaNombre = po.ClienteRealiza != null ? po.ClienteRealiza.nombre : "Desconocido",
                ClienteRealizaEmail = po.ClienteRealiza != null ? po.ClienteRealiza.email : "Desconocido",
                FechaPedido = po.fechaPedido,
                Estado = po.estado,
                Puntuacion = po.puntuacion,
                Comentario = po.comentario,
                FechaValoracion = po.fechaValoracion
            }).ToList();

            return Ok(pedidoResponses);
        }

        // Endpoint para obtener ofertas y pedidos creados/recibidos por una empresa específica
        // GET /api/PedidosOfertas/ByCompany/{id_empresa}
        [HttpGet("ByCompany/{id_empresa}")]
        [Authorize(Roles = "Empresa")] // Solo empresas autenticadas pueden ver sus ofertas/pedidos
        public async Task<ActionResult<IEnumerable<PedidoOfertaResponse>>> GetCompanyOffers(int id_empresa)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idEmpresaActual))
            {
                return Unauthorized("No se pudo identificar a la empresa actual.");
            }

            if (idEmpresaActual != id_empresa)
            {
                return Forbid("No tienes permiso para ver las ofertas/pedidos de otra empresa.");
            }

            // Obtener todas las PedidoOferta creadas por esta empresa
            var ofertasYPedidos = await _context.PedidosOfertas
                                                    .Where(po => po.id_empresa == id_empresa)
                                                    .Include(po => po.Receta) // Incluir la receta asociada
                                                    .Include(po => po.ClienteRealiza) // Incluir el cliente que hizo el pedido (si es un pedido)
                                                    .ToListAsync();

            // Mapear la entidad de dominio a PedidoOfertaResponse DTO
            var pedidoOfertaResponses = ofertasYPedidos.Select(po => new PedidoOfertaResponse
            {
                IdPedidoOferta = po.id_pedido_oferta,
                Precio = po.precio,
                Disponibilidad = po.disponibilidad,
                DescripcionOferta = po.descripcionOferta,
                IdEmpresa = po.id_empresa,
                EmpresaNombreNegocio = po.Empresa != null ? po.Empresa.nombre_negocio : "Desconocido", // Empresa ya está incluida por el filtro
                EmpresaUbicacion = po.Empresa != null ? po.Empresa.ubicacion : "Desconocido",
                IdReceta = po.id_receta,
                RecetaNombre = po.Receta != null ? po.Receta.nombre : "Desconocido",
                RecetaImagenUrl = po.Receta != null ? po.Receta.imagenUrl : null,
                IdClienteRealiza = po.id_cliente,
                ClienteRealizaNombre = po.ClienteRealiza != null ? po.ClienteRealiza.nombre : "Desconocido",
                ClienteRealizaEmail = po.ClienteRealiza != null ? po.ClienteRealiza.email : "Desconocido",
                FechaPedido = po.fechaPedido,
                Estado = po.estado,
                Puntuacion = po.puntuacion,
                Comentario = po.comentario,
                FechaValoracion = po.fechaValoracion
            }).ToList();

            return Ok(pedidoOfertaResponses);
        }

        // NUEVO: Endpoint para obtener todas las ofertas disponibles para una Receta específica
        // GET /api/PedidosOfertas/ByReceta/{id_receta}
        [HttpGet("ByReceta/{id_receta}")]
        // Este endpoint es público para que cualquier usuario (cliente o empresa) pueda ver las ofertas de una receta.
        public async Task<ActionResult<IEnumerable<PedidoOfertaResponse>>> GetOffersByReceta(int id_receta)
        {
            // Obtener solo las PedidoOferta que son "Oferta" (id_cliente es null) y están disponibles
            var ofertas = await _context.PedidosOfertas
                                        .Where(po => po.id_receta == id_receta && po.id_cliente == null && po.disponibilidad)
                                        .Include(po => po.Empresa) // Incluir la empresa que hace la oferta
                                        .Include(po => po.Receta)  // Incluir la receta asociada
                                        .ToListAsync();

            // Mapear la entidad de dominio a PedidoOfertaResponse DTO
            var ofertaResponses = ofertas.Select(po => new PedidoOfertaResponse
            {
                IdPedidoOferta = po.id_pedido_oferta,
                Precio = po.precio,
                Disponibilidad = po.disponibilidad,
                DescripcionOferta = po.descripcionOferta,
                IdEmpresa = po.id_empresa,
                EmpresaNombreNegocio = po.Empresa != null ? po.Empresa.nombre_negocio : "Desconocido",
                EmpresaUbicacion = po.Empresa != null ? po.Empresa.ubicacion : "Desconocido",
                IdReceta = po.id_receta,
                RecetaNombre = po.Receta != null ? po.Receta.nombre : "Desconocido",
                RecetaImagenUrl = po.Receta != null ? po.Receta.imagenUrl : null,
                FechaPedido = po.fechaPedido, // Será null para ofertas
                Estado = po.estado // Debería ser "Oferta"
            }).ToList();

            return Ok(ofertaResponses);
        }

        // NUEVO: Endpoint para que una Empresa cree una Oferta para una Receta
        // POST /api/PedidosOfertas/offer/{id_receta}
        [HttpPost("offer/{id_receta}")]
        [Authorize(Roles = "Empresa")] // Solo empresas pueden crear ofertas
        public async Task<ActionResult<PedidoOfertaResponse>> CreateOffer(int id_receta, [FromBody] OfertaRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idEmpresaActual))
            {
                return Unauthorized("No se pudo identificar a la empresa.");
            }

            var receta = await _context.Recetas.FindAsync(id_receta);
            if (receta == null)
            {
                return NotFound("Receta no encontrada.");
            }

            // Verificar si la empresa ya tiene una oferta activa para esta receta
            var existingOffer = await _context.PedidosOfertas
                                            .FirstOrDefaultAsync(po => po.id_empresa == idEmpresaActual && po.id_receta == id_receta && po.id_cliente == null);
            if (existingOffer != null)
            {
                return Conflict("Ya tienes una oferta activa para esta receta. Por favor, edítala si deseas cambiarla.");
            }


            var newOffer = new PedidoOferta
            {
                precio = request.Precio,
                disponibilidad = request.Disponibilidad,
                descripcionOferta = request.DescripcionOferta,
                id_empresa = idEmpresaActual, // Asignado desde el token
                id_receta = id_receta, // Asignado desde la URL
                id_cliente = null, // Es una oferta, no un pedido
                fechaPedido = null, // Es una oferta
                estado = "Oferta" // Estado inicial
            };

            _context.PedidosOfertas.Add(newOffer);
            await _context.SaveChangesAsync();

            // Cargar propiedades de navegación para el DTO de respuesta
            await _context.Entry(newOffer).Reference(po => po.Empresa).LoadAsync();
            await _context.Entry(newOffer).Reference(po => po.Receta).LoadAsync();

            var offerResponse = new PedidoOfertaResponse
            {
                IdPedidoOferta = newOffer.id_pedido_oferta,
                Precio = newOffer.precio,
                Disponibilidad = newOffer.disponibilidad,
                DescripcionOferta = newOffer.descripcionOferta,
                IdEmpresa = newOffer.id_empresa,
                EmpresaNombreNegocio = newOffer.Empresa != null ? newOffer.Empresa.nombre_negocio : "Desconocido",
                EmpresaUbicacion = newOffer.Empresa != null ? newOffer.Empresa.ubicacion : "Desconocido",
                IdReceta = newOffer.id_receta,
                RecetaNombre = newOffer.Receta != null ? newOffer.Receta.nombre : "Desconocido",
                RecetaImagenUrl = newOffer.Receta != null ? newOffer.Receta.imagenUrl : null,
                Estado = newOffer.estado
            };

            return CreatedAtAction(nameof(GetOffersByReceta), new { id_receta = offerResponse.IdReceta }, offerResponse);
        }

        // NUEVO: Endpoint para que un Cliente haga un Pedido de una Oferta existente
        // POST /api/PedidosOfertas/order
        [HttpPost("order")]
        [Authorize(Roles = "Cliente")] // Solo clientes pueden hacer pedidos
        public async Task<ActionResult<PedidoOfertaResponse>> PlaceOrder([FromBody] PedidoRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente.");
            }

            // Buscar la oferta, asegurándose de que esté disponible y no sea ya un pedido
            var offer = await _context.PedidosOfertas
                                      .Include(po => po.Empresa)
                                      .Include(po => po.Receta)
                                      .Include(po => po.ClienteRealiza) // Para el mapeo de respuesta
                                      .FirstOrDefaultAsync(po => po.id_pedido_oferta == request.IdPedidoOferta && po.id_cliente == null && po.disponibilidad);

            if (offer == null)
            {
                return NotFound("Oferta no encontrada, no disponible o ya es un pedido.");
            }

            // Convertir la oferta en un pedido
            offer.id_cliente = idClienteActual; // Asignar el cliente que realiza el pedido
            offer.fechaPedido = DateTime.UtcNow;
            offer.estado = "Pedido_Pendiente"; // Cambiar estado

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PedidosOfertas.Any(po => po.id_pedido_oferta == request.IdPedidoOferta))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al realizar pedido: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al realizar el pedido.");
            }

            // Cargar el ClienteRealiza para el DTO de respuesta (si no se cargó con el Include)
            if (offer.ClienteRealiza == null && offer.id_cliente.HasValue)
            {
                await _context.Entry(offer).Reference(po => po.ClienteRealiza).LoadAsync();
            }

            var orderResponse = new PedidoOfertaResponse
            {
                IdPedidoOferta = offer.id_pedido_oferta,
                Precio = offer.precio,
                Disponibilidad = offer.disponibilidad,
                DescripcionOferta = offer.descripcionOferta,
                IdEmpresa = offer.id_empresa,
                EmpresaNombreNegocio = offer.Empresa != null ? offer.Empresa.nombre_negocio : "Desconocido",
                EmpresaUbicacion = offer.Empresa != null ? offer.Empresa.ubicacion : "Desconocido",
                IdReceta = offer.id_receta,
                RecetaNombre = offer.Receta != null ? offer.Receta.nombre : "Desconocido",
                RecetaImagenUrl = offer.Receta != null ? offer.Receta.imagenUrl : null,
                IdClienteRealiza = offer.id_cliente,
                ClienteRealizaNombre = offer.ClienteRealiza != null ? offer.ClienteRealiza.nombre : "Desconocido",
                ClienteRealizaEmail = offer.ClienteRealiza != null ? offer.ClienteRealiza.email : "Desconocido",
                FechaPedido = offer.fechaPedido,
                Estado = offer.estado,
                Puntuacion = offer.puntuacion,
                Comentario = offer.comentario,
                FechaValoracion = offer.fechaValoracion
            };

            return Ok(orderResponse); // Devuelve el pedido actualizado
        }

        // NUEVO: Endpoint para que una Empresa marque un Pedido como Completado
        // PUT /api/PedidosOfertas/complete/{id_pedido_oferta}
        [HttpPut("complete/{id_pedido_oferta}")]
        [Authorize(Roles = "Empresa")] // Solo la empresa puede marcar como completado
        public async Task<IActionResult> CompleteOrder(int id_pedido_oferta)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idEmpresaActual))
            {
                return Unauthorized("No se pudo identificar a la empresa.");
            }

            var pedido = await _context.PedidosOfertas.FindAsync(id_pedido_oferta);

            if (pedido == null)
            {
                return NotFound("Pedido no encontrado.");
            }

            // Verificar que el pedido pertenece a la empresa logueada
            if (pedido.id_empresa != idEmpresaActual)
            {
                return Forbid("No tienes permiso para marcar este pedido como completado.");
            }

            // Solo se puede completar un pedido pendiente
            if (pedido.estado != "Pedido_Pendiente")
            {
                return BadRequest("Solo los pedidos pendientes pueden ser marcados como completados.");
            }

            pedido.estado = "Pedido_Completado";
            // No se guarda fecha de completado, pero se podría añadir un campo si es necesario.

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PedidosOfertas.Any(po => po.id_pedido_oferta == id_pedido_oferta))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al completar pedido: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al completar el pedido.");
            }

            return NoContent(); // 204 No Content
        }

        // NUEVO: Endpoint para que un Cliente valore un Pedido Completado
        // PUT /api/PedidosOfertas/rate/{id_pedido_oferta}
        [HttpPut("rate/{id_pedido_oferta}")]
        [Authorize(Roles = "Cliente")] // Solo el cliente puede valorar
        public async Task<IActionResult> RateOrder(int id_pedido_oferta, [FromBody] ValoracionRequest request) // Necesitarás ValoracionRequest DTO
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente.");
            }

            var pedido = await _context.PedidosOfertas.FindAsync(id_pedido_oferta);

            if (pedido == null)
            {
                return NotFound("Pedido no encontrado.");
            }

            // Verificar que el pedido pertenece al cliente logueado
            if (pedido.id_cliente != idClienteActual)
            {
                return Forbid("No tienes permiso para valorar este pedido.");
            }

            // Solo se puede valorar un pedido completado y que no haya sido valorado
            if (pedido.estado != "Pedido_Completado" || pedido.puntuacion.HasValue)
            {
                return BadRequest("Solo los pedidos completados y no valorados pueden ser valorados.");
            }

            if (request.Puntuacion < 1 || request.Puntuacion > 5)
            {
                return BadRequest("La puntuación debe estar entre 1 y 5.");
            }

            pedido.puntuacion = request.Puntuacion;
            pedido.comentario = request.Comentario;
            pedido.fechaValoracion = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PedidosOfertas.Any(po => po.id_pedido_oferta == id_pedido_oferta))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al valorar pedido: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al valorar el pedido.");
            }

            return NoContent(); // 204 No Content
        }

        // NUEVO: Endpoint para eliminar una oferta
        // DELETE /api/PedidosOfertas/{id_pedido_oferta}
        [HttpDelete("{id_pedido_oferta}")]
        [Authorize(Roles = "Empresa")] // Solo la empresa que la creó puede eliminarla
        public async Task<IActionResult> DeleteOffer(int id_pedido_oferta)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idEmpresaActual))
            {
                return Unauthorized("No se pudo identificar a la empresa.");
            }

            var offerToDelete = await _context.PedidosOfertas.FindAsync(id_pedido_oferta);

            if (offerToDelete == null)
            {
                return NotFound("Oferta no encontrada.");
            }

            // Verificar que la oferta pertenece a la empresa logueada y que es una oferta (no un pedido ya)
            if (offerToDelete.id_empresa != idEmpresaActual || offerToDelete.id_cliente.HasValue)
            {
                return Forbid("No tienes permiso para eliminar esta oferta o ya es un pedido.");
            }

            _context.PedidosOfertas.Remove(offerToDelete);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
    }
}