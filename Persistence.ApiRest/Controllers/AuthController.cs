
using Microsoft.AspNetCore.Mvc;
using Domain.Model; // Para entidades Cliente y Empresa (si las usas en la respuesta)
using Domain.Model.ApiRequests; // Para LoginRequest, LoginResponse
using Persistence.ApiRest; // Para ApplicationDbContext
using Microsoft.EntityFrameworkCore; // Para FirstOrDefaultAsync
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt; // Para JWT
using System.Security.Claims; // Para Claims
using Microsoft.IdentityModel.Tokens; // Para SecurityTokenDescriptor
using System.Text; // Para Encoding
using Microsoft.Extensions.Configuration;

namespace Persistence.ApiRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Ruta base: /api/Auth
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration; // Para acceder a la configuracion (JWT Secret)

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Endpoint para iniciar sesion
        // POST /api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email y contraseña son obligatorios.");
            }

            // Buscar cliente
            var cliente = await _context.Clientes
                                        .FirstOrDefaultAsync(c => c.email == request.Email && c.password_hash == request.Password);

            if (cliente != null)
            {
                var token = GenerateJwtToken(cliente.id_cliente, "Cliente");
                // Conversion explicita a int para UserId
                return Ok(new LoginResponse { Token = token, UserType = "Cliente", UserId = cliente.id_cliente });
            }

            // Buscar empresa
            var empresa = await _context.Empresas
                                        .FirstOrDefaultAsync(e => e.email == request.Email && e.password_hash == request.Password);

            if (empresa != null)
            {
                var token = GenerateJwtToken(empresa.id_empresa, "Empresa");
                // Conversion explicita a int para UserId
                return Ok(new LoginResponse { Token = token, UserType = "Empresa", UserId = empresa.id_empresa });
            }

            return Unauthorized("Credenciales inválidas.");
        }
        // Endpoint para registrar un cliente
        [HttpPost("register/cliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] ClienteRegistrationRequest request)
        {
            // PON EL BREAKPOINT AQUÍ (al principio del método)
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Ubicacion))
            {
                return BadRequest("Todos los campos de cliente son obligatorios.");
            }

            // Verificar si el email ya existe para un cliente
            if (await _context.Clientes.AnyAsync(c => c.email == request.Email))
            { // request.Email (DTO PascalCase) vs c.email (Model lowercase)
                return BadRequest("El email ya está registrado como cliente.");
            }

            // Verificar si el email ya existe para una empresa
            if (await _context.Empresas.AnyAsync(e => e.email == request.Email))
            { // request.Email (DTO PascalCase) vs e.email (Model lowercase)
                return BadRequest("El email ya está registrado como empresa.");
            }

            var newClient = new Cliente
            {
                email = request.Email,          // Mapeo: DTO.Email (PascalCase) -> Model.email (lowercase)
                password_hash = request.Password, // Mapeo: DTO.Password (PascalCase) -> Model.password_hash
                nombre = request.Nombre,        // Mapeo: DTO.Nombre (PascalCase) -> Model.nombre (lowercase)
                ubicacion = request.Ubicacion   // Mapeo: DTO.Ubicacion (PascalCase) -> Model.ubicacion (lowercase)
            };

            _context.Clientes.Add(newClient);
            await _context.SaveChangesAsync(); // POSIBLE LUGAR DE ERROR: PON OTRO BREAKPOINT AQUÍ

            return Ok(new { Message = "Registro de cliente exitoso", UserId = newClient.id_cliente });
        }

        // Endpoint para registrar una empresa
        [HttpPost("register/empresa")]
        public async Task<IActionResult> RegisterEmpresa([FromBody] EmpresaRegistrationRequest request)
        {
            // PON EL BREAKPOINT AQUÍ (al principio del método)
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.NombreNegocio) || string.IsNullOrWhiteSpace(request.Descripcion) || // ¡Usar NombreNegocio aquí!
                string.IsNullOrWhiteSpace(request.Ubicacion))
            {
                return BadRequest("Todos los campos de empresa son obligatorios.");
            }

            // Verificar si el email ya existe para un cliente
            if (await _context.Clientes.AnyAsync(c => c.email == request.Email))
            {
                return BadRequest("El email ya está registrado como cliente.");
            }

            // Verificar si el email ya existe para una empresa
            if (await _context.Empresas.AnyAsync(e => e.email == request.Email))
            {
                return BadRequest("El email ya está registrado como empresa.");
            }

            var newEmpresa = new Empresa
            {
                email = request.Email,
                password_hash = request.Password,
                nombre_negocio = request.NombreNegocio, // ¡Mapeo: DTO.NombreNegocio -> Model.nombre_negocio!
                descripcion = request.Descripcion,
                ubicacion = request.Ubicacion
            };

            _context.Empresas.Add(newEmpresa);
            await _context.SaveChangesAsync(); // POSIBLE LUGAR DE ERROR: PON OTRO BREAKPOINT AQUÍ

            return Ok(new { Message = "Registro de empresa exitoso", UserId = newEmpresa.id_empresa });
        }

        // Metodo para generar un token JWT
        private string GenerateJwtToken(int userId, string userType)
        {
            // Obtener la clave secreta de la configuracion (ej. appsettings.json o variables de entorno)
            var jwtSecret = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret no configurado. Añade 'Jwt:Key' a tu appsettings.json o variables de entorno.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, userType), // Rol del usuario (Cliente/Empresa)
                new Claim(ClaimTypes.Email, userType == "Cliente" ? _context.Clientes.Find(userId)?.email : _context.Empresas.Find(userId)?.email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token valido por 1 hora
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
