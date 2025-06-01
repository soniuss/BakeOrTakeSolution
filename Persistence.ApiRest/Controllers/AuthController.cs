using Microsoft.AspNetCore.Mvc;
using Domain.Model; // Para entidades Cliente y Empresa (si las usas en la respuesta)
using Domain.Model.ApiRequests; // Para LoginRequest, LoginResponse, ClienteRegistrationRequest, EmpresaRegistrationRequest
using Persistence.ApiRest; // Para ApplicationDbContext
using Microsoft.EntityFrameworkCore; // Para FirstOrDefaultAsync, AnyAsync, SaveChangesAsync
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt; // Para JWT
using System.Security.Claims; // Para Claims
using Microsoft.IdentityModel.Tokens; // Para SecurityTokenDescriptor
using System.Text; // Para Encoding
using Microsoft.Extensions.Configuration; // Para IConfiguration
using System; // ¡Necesario para Exception y Console.WriteLine!

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
            // Aunque este método no parece ser el que falla (es el registro), es buena práctica envolverlo en un try-catch también
            try
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
                    return Ok(new LoginResponse { Token = token, UserType = "Cliente", UserId = cliente.id_cliente });
                }

                // Buscar empresa
                var empresa = await _context.Empresas
                                            .FirstOrDefaultAsync(e => e.email == request.Email && e.password_hash == request.Password);

                if (empresa != null)
                {
                    var token = GenerateJwtToken(empresa.id_empresa, "Empresa");
                    return Ok(new LoginResponse { Token = token, UserType = "Empresa", UserId = empresa.id_empresa });
                }

                return Unauthorized("Credenciales inválidas.");
            }
            catch (Exception ex)
            {
                // Este catch es por si algo falla en la lógica de Login
                Console.WriteLine($"------------- ERROR EN API REST (LOGIN) -------------");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"-----------------------------------------------------");

                // --- ¡¡TEMPORAL PARA DEPURACIÓN!! ---
                return StatusCode(500, new // Devuelve un StatusCode 500 con un cuerpo JSON de error
                {
                    Message = "Error interno del servidor al iniciar sesión",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerExceptionMessage = ex.InnerException?.Message
                });
                // --- ¡¡FIN TEMPORAL!! ---
            }
        }

        // Endpoint para registrar un cliente
        [HttpPost("register/cliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] ClienteRegistrationRequest request)
        {
            // --- INICIO DEL TRY-CATCH ---
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.Ubicacion))
                {
                    return BadRequest("Todos los campos de cliente son obligatorios.");
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

                var newClient = new Cliente
                {
                    email = request.Email,
                    password_hash = request.Password,
                    nombre = request.Nombre,
                    ubicacion = request.Ubicacion
                };

                _context.Clientes.Add(newClient);
                await _context.SaveChangesAsync(); // <-- Si hay un error aquí, ahora será capturado

                return Ok(new { Message = "Registro de cliente exitoso", UserId = newClient.id_cliente });
            }
            catch (Exception ex)
            {
                // --- ¡¡TEMPORAL PARA DEPURACIÓN!! ---
                Console.WriteLine($"------------- ERROR EN API REST (CLIENTE) -------------");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}"); // Útil si hay una excepción anidada
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"-----------------------------------------------------");

                return StatusCode(500, new // Devuelve un StatusCode 500 con un cuerpo JSON de error
                {
                    Message = "Error interno del servidor al registrar cliente",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerExceptionMessage = ex.InnerException?.Message
                });
                // --- ¡¡FIN TEMPORAL!! ---
            }
            // --- FIN DEL TRY-CATCH ---
        }

        // Endpoint para registrar una empresa
        [HttpPost("register/empresa")]
        public async Task<IActionResult> RegisterEmpresa([FromBody] EmpresaRegistrationRequest request)
        {
            // --- INICIO DEL TRY-CATCH ---
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.NombreNegocio) || string.IsNullOrWhiteSpace(request.Descripcion) ||
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
                await _context.SaveChangesAsync(); // <-- Si hay un error aquí, ahora será capturado

                return Ok(new { Message = "Registro de empresa exitoso", UserId = newEmpresa.id_empresa });
            }
            catch (Exception ex)
            {
                // --- ¡¡TEMPORAL PARA DEPURACIÓN!! ---
                Console.WriteLine($"------------- ERROR EN API REST (EMPRESA) -------------");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"-----------------------------------------------------");

                return StatusCode(500, new // Devuelve un StatusCode 500 con un cuerpo JSON de error
                {
                    Message = "Error interno del servidor al registrar empresa",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerExceptionMessage = ex.InnerException?.Message
                });
                // --- ¡¡FIN TEMPORAL!! ---
            }
            // --- FIN DEL TRY-CATCH ---
        }

        // Metodo para generar un token JWT
        private string GenerateJwtToken(int userId, string userType)
        {
            // Aunque este método no parece ser el que falla en el registro,
            // es buena práctica envolverlo en un try-catch si tiene lógica compleja.
            try
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
                    // Este Find puede lanzar una excepción si userId no existe, aunque el registro debería garantizarlo
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
            catch (Exception ex)
            {
                Console.WriteLine($"------------- ERROR EN API REST (GENERATE JWT) -------------");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"-----------------------------------------------------");
                // Lanzar la excepción de nuevo o devolver un error específico si se desea
                throw; // Relanzar la excepción para que el método llamador (Login) la capture
            }
        }
    }
}