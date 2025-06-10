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
                password_hash = request.Password,
                nombre = request.Nombre,
                ubicacion = request.Ubicacion,
                fecha_registro = DateTime.UtcNow 
            };

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync();

            
            return Ok(nuevoCliente); 
        }

        // Ejemplo de endpoint GET para verificar
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
            return Ok(cliente);
        }
        // PUT /api/Clientes/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Cliente")] // Solo un cliente autenticado puede actualizar su propio perfil
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] Cliente updateData)
        {
            if (id != updateData.id_cliente)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del cliente en el cuerpo.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int idClienteActual))
            {
                return Unauthorized("No se pudo identificar al cliente actual.");
            }

            // Verificar que el ID solicitado coincide con el ID del token
            if (idClienteActual != id)
            {
                return Forbid("No tienes permiso para actualizar el perfil de otro cliente.");
            }

            var existingCliente = await _context.Clientes.FindAsync(id);
            if (existingCliente == null)
            {
                return NotFound();
            }

            // Actualizar solo los campos permitidos desde el cliente
            existingCliente.nombre = updateData.nombre;
            existingCliente.ubicacion = updateData.ubicacion;
           
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clientes.Any(c => c.id_cliente == id))
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
                Console.WriteLine($"Error al actualizar cliente: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al actualizar el cliente.");
            }

            return NoContent(); // 204 No Content para actualización exitosa
        }
    }
}