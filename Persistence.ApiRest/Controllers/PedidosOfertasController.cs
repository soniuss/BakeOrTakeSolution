// Si no tienes PedidosOfertasController, crea este archivo:
// Persistence.ApiRest/Controllers/PedidosOfertasController.cs

using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Necesario para Claims
using Microsoft.AspNetCore.Authorization; // Necesario para [Authorize]

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

            // Aquí puedes mapear a un DTO de respuesta más simple si no quieres exponer toda la entidad PedidoOferta
            // O puedes devolver la entidad directamente si no hay datos sensibles adicionales.
            return Ok(pedidos);
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

            var ofertas = await _context.PedidosOfertas
                                        .Include(po => po.Receta)
                                        .Include(po => po.ClienteRealiza) // Si quieres ver quién lo ha pedido
                                        .Where(po => po.id_empresa == id_empresa)
                                        .ToListAsync();
            return Ok(ofertas);
        }

        // Aquí irían otros métodos CRUD para PedidoOferta si los necesitas
        // (ej. POST para crear una oferta, PUT para actualizar el estado de un pedido, etc.)
    }
}