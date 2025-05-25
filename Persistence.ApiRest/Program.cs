using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Persistence.ApiRest;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configurar la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Intentar obtener DATABASE_URL de Railway
var railwayDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(railwayDatabaseUrl))
{
    // Si DATABASE_URL existe y no esta vacia, usarla y convertirla
    connectionString = ConvertirDatabaseUrlToMySqlConnectionString(railwayDatabaseUrl);
}
else
{
    // Si DATABASE_URL no existe o esta vacia, construir la cadena de conexion
    // usando las variables individuales de MySQL inyectadas por Railway
    // NOTA: Se mantienen los nombres de variables que te funcionan (sin guion bajo en algunos)
    var mysqlHost = Environment.GetEnvironmentVariable("MYSQLHOST");
    var mysqlPort = Environment.GetEnvironmentVariable("MYSQLPORT");
    var mysqlUser = Environment.GetEnvironmentVariable("MYSQLUSER");
    var mysqlPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD");
    var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQL_DATABASE"); // este SÍ lleva guion bajo


    if (
        !string.IsNullOrEmpty(mysqlHost) &&
        !string.IsNullOrEmpty(mysqlPort) &&
        !string.IsNullOrEmpty(mysqlUser) &&
        !string.IsNullOrEmpty(mysqlDatabase)
    )
    {
        // La contraseña puede ser nula si no hay una definida
        connectionString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDatabase};User={mysqlUser};Password={mysqlPassword};";
    }
    else
    {
        // Si ninguna variable de Railway esta disponible, se usara la de appsettings.json
        // (que ya es el valor por defecto de connectionString)
        Console.WriteLine("ADVERTENCIA: No se encontraron variables de entorno de Railway para MySQL. Usando DefaultConnection de appsettings.json.");
    }
}


// *** AÑADIR ESTA LINEA PARA LOGUEAR LA CADENA DE CONEXION FINAL ***
Console.WriteLine($"DEBUG: Usando cadena de conexion: {connectionString}");

// Añadir DbContext con MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Configuracion de Autenticacion JWT ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// *** INICIO DE LA CORRECCION PARA SWAGGER ***
// Mueve estas líneas FUERA del bloque if (app.Environment.IsDevelopment())
// para que Swagger UI esté disponible en todos los entornos (incluido Railway).
app.UseSwagger();
app.UseSwaggerUI();
// *** FIN DE LA CORRECCION PARA SWAGGER ***


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// **Aplicar migraciones antes de empezar el pipeline**
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("DEBUG: Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR: Fallo al aplicar migraciones: {ex.Message}");
        // Opcional: relanzar la excepcion si quieres que el despliegue falle
        // throw;
    }
}

// Obtener puerto de variable de entorno (Railway)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");


app.Run();

// Método para convertir DATABASE_URL a cadena de conexión MySQL compatible
string ConvertirDatabaseUrlToMySqlConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    var user = userInfo[0];
    var password = userInfo[1];
    var host = uri.Host;
    var port = uri.Port;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Server={host};Port={port};Database={database};User={user};Password={password};";
}
