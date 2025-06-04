// Si no tienes PedidosOfertasController, crea este archivo:
// Persistence.ApiRest/Controllers/PedidosOfertasController.cs

using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Necesario para Claims
using Microsoft.AspNetCore.Authorization;
using Domain.Model.ApiResponses;
using Domain.Model.ApiRequests; // Necesario para [Authorize]

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

        // Opcional: Endpoint para obtener todos los PedidoOferta (con cuidado de los permisos)
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<PedidoOferta>>> GetPedidosOfertas()
        // {
        //     return Ok(await _context.PedidosOfertas.ToListAsync());
        // }

        // NUEVO: Endpoint para obtener pedidos realizados por un cliente específico
        // GET /api/PedidosOfertas/ByClient/{id_cliente}
        [HttpGet("ByClient/{id_cliente}")]
        [Authorize(Roles = "Cliente")] // Solo clientes autenticados pueden ver sus pedidos
        public async Task<ActionResult<IEnumerable<PedidoOferta>>> GetClientOrders(int id_cliente)
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

            // Incluir las propiedades de navegación necesarias
            var pedidos = await _context.PedidosOfertas
                                        .Include(po => po.Empresa) // Para obtener el nombre de la empresa
                                        .Include(po => po.Receta)  // Para obtener el nombre de la receta
                                        .Where(po => po.id_cliente == id_cliente) // Filtra por el cliente que REALIZÓ el pedido
                                        .ToListAsync();

            // Mapear a PedidoOfertaResponse
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

        // Opcional: Endpoint para obtener ofertas creadas por una empresa específica
        // GET /api/PedidosOfertas/ByCompany/{id_empresa}
        [HttpGet("ByCompany/{id_empresa}")]
        [Authorize(Roles = "Empresa")] // Solo empresas autenticadas pueden ver sus ofertas/pedidos
        public async Task<ActionResult<IEnumerable<PedidoOferta>>> GetCompanyOffers(int id_empresa)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idEmpresaActual))
            {
                return Unauthorized("No se pudo identificar a la empresa actual.");
            }

            if (idEmpresaActual != id_empresa)
            {
                return Forbid("No tienes permiso para ver las ofertas de otra empresa.");
            }

            var ofertasYPedidos = await _context.PedidosOfertas
                                        .Include(po => po.Receta)
                                        .Include(po => po.ClienteRealiza) // Si quieres ver quién lo ha pedido
                                        .Where(po => po.id_empresa == id_empresa)
                                        .ToListAsync();
            // Mapear a PedidoOfertaResponse
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

        // NUEVO: Endpoint para obtener todas las ofertas para una Receta específica
        // GET /api/PedidosOfertas/ByReceta/{id_receta}
        [HttpGet("ByReceta/{id_receta}")]
        // Esta ruta puede ser pública si quieres que cualquiera vea las ofertas de una receta,
        // o protegida si solo usuarios logueados pueden verlas.
        // Por ahora, la haremos pública para la página de detalle de receta.
        public async Task<ActionResult<IEnumerable<PedidoOfertaResponse>>> GetOffersByReceta(int id_receta)
        {
            var ofertas = await _context.PedidosOfertas
                                        .Where(po => po.id_receta == id_receta && po.id_cliente == null) // Solo ofertas (sin cliente asignado)
                                        .Include(po => po.Empresa)
                                        .Include(po => po.Receta)
                                        .ToListAsync();

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

            var offer = await _context.PedidosOfertas
                                      .Include(po => po.Empresa)
                                      .Include(po => po.Receta)
                                      .FirstOrDefaultAsync(po => po.id_pedido_oferta == request.IdPedidoOferta && po.id_cliente == null); // Asegurarse de que es una oferta activa

            if (offer == null)
            {
                return NotFound("Oferta no encontrada o ya es un pedido.");
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

            // Cargar el ClienteRealiza para el DTO de respuesta
            await _context.Entry(offer).Reference(po => po.ClienteRealiza).LoadAsync();

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
                Estado = offer.estado
            };

            return Ok(orderResponse); // Devuelve el pedido actualizado
        }
    }
}