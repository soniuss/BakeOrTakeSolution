using Domain.Model.ApiRequests;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}
