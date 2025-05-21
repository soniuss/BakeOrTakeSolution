using Persistence.ApiRest;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                             ?? Environment.GetEnvironmentVariable("DATABASE_URL");

// Si DATABASE_URL viene en formato Railway mysql://usuario:clave@host:puerto/db,
// necesitas convertirla a formato para Pomelo MySQL
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("mysql://"))
{
    connectionString = ConvertirDatabaseUrlToMySqlConnectionString(connectionString);
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
    // db.Database.Migrate(); // Mantenemos esta linea
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
