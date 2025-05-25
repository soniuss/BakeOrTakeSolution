
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
