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
