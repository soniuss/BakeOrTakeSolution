using Domain.Model.ApiRequests;
using Domain.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Persistence.ApiRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta base: /api/Empresas
    public class EmpresasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmpresasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint GET para obtener una empresa por ID
        // GET /api/Empresas/{id}
        // [Authorize(Roles = "Empresa")] // Opcional: solo empresas autenticadas pueden ver su propio perfil
        [HttpGet("{id}")]
        public async Task<ActionResult<Empresa>> GetEmpresa(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound();
            }

            // Opcional: Verificar que el ID del token coincide con el ID solicitado si es para perfil propio
            // var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (userIdFromToken != null && int.Parse(userIdFromToken) != id)
            // {
            //     return Forbid(); // O Unauthorized
            // }

            return Ok(empresa);
        }

        // Endpoint PUT para actualizar una empresa por ID
        // PUT /api/Empresas/{id}
        // [Authorize(Roles = "Empresa")] // Opcional: solo empresas autenticadas pueden actualizar su propio perfil
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmpresa(int id, [FromBody] Empresa updateData)
        {
            if (id != updateData.id_empresa)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del cuerpo.");
            }

            var existingEmpresa = await _context.Empresas.FindAsync(id);
            if (existingEmpresa == null)
            {
                return NotFound();
            }
            // Opcional: Verificar que el ID del token coincide con el ID solicitado
            // var userIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // if (userIdFromToken != null && int.Parse(userIdFromToken) != id)
            // {
            //     return Forbid(); // O Unauthorized
            // }

            // Actualizar solo los campos permitidos
            existingEmpresa.email = updateData.email;
            existingEmpresa.nombre_negocio = updateData.nombre_negocio;
            existingEmpresa.descripcion = updateData.descripcion;
            existingEmpresa.ubicacion = updateData.ubicacion;
            // No actualizar password_hash directamente aquí; debe ser un endpoint separado y seguro.
            // No actualizar fecha_registro

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Empresas.Any(e => e.id_empresa == id))
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
                Console.WriteLine($"Error al actualizar empresa: {ex.Message}");
                return StatusCode(500, "Error interno del servidor al actualizar la empresa.");
            }

            return NoContent(); // 204 No Content para actualización exitosa
        }
        // Endpoint para registrar una nueva empresa
        // POST /api/Empresas/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterEmpresa([FromBody] EmpresaRegistrationRequest request)
        {
            // Validacion basica
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.NombreNegocio) || string.IsNullOrWhiteSpace(request.Ubicacion) ||
                string.IsNullOrWhiteSpace(request.Descripcion))
            {
                return BadRequest("Todos los campos son obligatorios.");
            }

            // Verificar si el email ya existe
            if (await _context.Empresas.AnyAsync(e => e.email == request.Email))
            {
                return Conflict("El email ya está registrado.");
            }

            // Crear una nueva entidad Empresa
            var nuevaEmpresa = new Empresa
            {
                email = request.Email,
                password_hash = request.Password, // Deberia ser un hash
                nombre_negocio = request.NombreNegocio,
                descripcion = request.Descripcion,
                ubicacion = request.Ubicacion,
                fecha_registro = DateTime.UtcNow
            };

            _context.Empresas.Add(nuevaEmpresa);
            await _context.SaveChangesAsync();

            return Ok(nuevaEmpresa);
        }

        // Ejemplo de endpoint GET para verificar (solo para desarrollo/prueba)
        // GET /api/Empresas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
        {
            return Ok(await _context.Empresas.ToListAsync());
        }
    }
}
