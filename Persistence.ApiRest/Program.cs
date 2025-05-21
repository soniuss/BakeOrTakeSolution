using Persistence.ApiRest;
using Microsoft.EntityFrameworkCore;

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
    var mysqlHost = Environment.GetEnvironmentVariable("MYSQL_HOST");
    var mysqlPort = Environment.GetEnvironmentVariable("MYSQL_PORT");
    var mysqlUser = Environment.GetEnvironmentVariable("MYSQL_USER");
    var mysqlPassword = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
    var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQL_DATABASE");

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

var app = builder.Build();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

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
