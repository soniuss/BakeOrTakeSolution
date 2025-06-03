using Domain.Model.ApiRequests;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Persistence.ApiRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta base: /api/Clientes
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Inyeccion de dependencia del DbContext
        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint para registrar un nuevo cliente
        // POST /api/Clientes/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterCliente([FromBody] ClienteRegistrationRequest request)
        {
            // Validacion basica de datos de entrada
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Ubicacion))
            {
                return BadRequest("Todos los campos son obligatorios.");
            }

            // Verificar si el email ya existe
            if (await _context.Clientes.AnyAsync(c => c.email == request.Email))
            {
                return Conflict("El email ya está registrado.");
            }

            // Crear una nueva entidad Cliente
            var nuevoCliente = new Cliente
            {
                email = request.Email,
                // NOTA: En una aplicacion real, la contraseña DEBE ser hasheada y no guardada en texto plano.
                // Esto es solo para propositos de prueba inicial.
                password_hash = request.Password, // Deberia ser un hash
                nombre = request.Nombre,
                ubicacion = request.Ubicacion,
                fecha_registro = DateTime.UtcNow // O DateTime.Now segun tu preferencia
            };

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync();

            // Devolver una respuesta exitosa, por ejemplo, el cliente creado
            // Podrias devolver un DTO de respuesta mas simple si no quieres exponer todo el modelo
            return Ok(nuevoCliente); // O CreatedAtAction para RESTful completo
        }

        // Ejemplo de endpoint GET para verificar (solo para desarrollo/prueba)
        // GET /api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return Ok(await _context.Clientes.ToListAsync());
        }

        // GET /api/Clientes/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Cliente")] // Solo un cliente autenticado puede ver su propio perfil
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            // Obtener el ID del cliente del token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            // Verificar que el ID solicitado coincide con el ID del token
            if (idClienteActual != id)
            {
                return Forbid("No tienes permiso para ver el perfil de otro cliente.");
            }

            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            // Opcional: Si Cliente tuviera propiedades de navegación cíclicas,
            // necesitaríamos un ClienteResponse DTO aquí para evitar errores de serialización.
            // Por ahora, asumimos que Cliente se puede serializar directamente.
            return Ok(cliente);
        }
    }
}
