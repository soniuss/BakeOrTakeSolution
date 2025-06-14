﻿using Microsoft.AspNetCore.Mvc;
using Domain.Model; 
using Domain.Model.ApiRequests; 
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt; 
using System.Security.Claims; 
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Email y contraseña son obligatorios.");
                }

                // Buscar cliente por email (SOLO POR EMAIL, AUN NO POR CONTRASEÑA)
                var cliente = await _context.Clientes
                                            .FirstOrDefaultAsync(c => c.email == request.Email); 

                if (cliente != null)
                {
                    // VERIFICAR CONTRASEÑA HASHEADA
                    if (BCrypt.Net.BCrypt.Verify(request.Password, cliente.password_hash)) 
                    {
                        var token = GenerateJwtToken(cliente.id_cliente, "Cliente");
                        return Ok(new LoginResponse { Token = token, UserType = "Cliente", UserId = cliente.id_cliente });
                    }
                }

                // Buscar empresa por email (SOLO POR EMAIL, AUN NO POR CONTRASEÑA)
                var empresa = await _context.Empresas
                                            .FirstOrDefaultAsync(e => e.email == request.Email); 

                if (empresa != null)
                {
                    // VERIFICAR CONTRASEÑA HASHEADA
                    if (BCrypt.Net.BCrypt.Verify(request.Password, empresa.password_hash)) 
                    {
                        var token = GenerateJwtToken(empresa.id_empresa, "Empresa");
                        return Ok(new LoginResponse { Token = token, UserType = "Empresa", UserId = empresa.id_empresa });
                    }
                }

                // Si no se encontró ningún usuario con ese email o la contraseña no coincide
                return Unauthorized("Credenciales inválidas.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"------------- ERROR EN API REST (LOGIN) -------------");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"-----------------------------------------------------");

                return StatusCode(500, new
                {
                    Message = "Error interno del servidor al iniciar sesión",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace,
                    InnerExceptionMessage = ex.InnerException?.Message
                });
            }
        }

        // Endpoint para registrar un cliente
        [HttpPost("register/cliente")]
        public async Task<IActionResult> RegisterCliente([FromBody] ClienteRegistrationRequest request)
        {
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
                    password_hash = BCrypt.Net.BCrypt.HashPassword(request.Password), 
                    nombre = request.Nombre,
                    ubicacion = request.Ubicacion,
                    fecha_registro = DateTime.UtcNow 
                };

                _context.Clientes.Add(newClient);
                await _context.SaveChangesAsync(); 

                return Ok(new { Message = "Registro de cliente exitoso", UserId = newClient.id_cliente });
            }
            catch (Exception ex)
            {
                // --- ¡¡TEMPORAL PARA DEPURACIÓN!! ---
                Console.WriteLine($"------------- ERROR EN API REST (CLIENTE) -------------");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}"); 
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
        }

        // Endpoint para registrar una empresa
        [HttpPost("register/empresa")]
        public async Task<IActionResult> RegisterEmpresa([FromBody] EmpresaRegistrationRequest request)
        {
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
                    password_hash = BCrypt.Net.BCrypt.HashPassword(request.Password), 
                    nombre_negocio = request.NombreNegocio,
                    descripcion = request.Descripcion,
                    ubicacion = request.Ubicacion,
                    fecha_registro = DateTime.UtcNow 
                };

                _context.Empresas.Add(newEmpresa);
                await _context.SaveChangesAsync(); 

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
        }

        // Metodo para generar un token JWT
        private string GenerateJwtToken(int userId, string userType)
        {
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