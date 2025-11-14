using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Energix.API;
using Energix.API.DeviceManagement.Application.Internal.CommandServices;
using Energix.API.DeviceManagement.Application.Internal.QueryServices;
using Energix.API.DeviceManagement.Domain.Repositories;
using Energix.API.DeviceManagement.Domain.Services;
using Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Repositories;
using Energix.API.DeviceManagement.Infrastructure.Services;
using Energix.API.Personalization.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI y explorador de endpoints
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());

// Controllers
builder.Services.AddControllers();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
              ?? "server=localhost;port=3306;database=energix;user=root;password=Password123;TreatTinyAsBoolean=true";

// Usar una versión específica de MySQL en lugar de AutoDetect para evitar conexión en tiempo de configuración
var serverVersion = new MySqlServerVersion(new Version(8, 0, 21));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connStr, serverVersion, mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    }));

// Personalization Services
builder.Services.AddPersonalizationServices();

// Domain Services
builder.Services.AddScoped<IPlanValidationService, PlanValidationService>();
builder.Services.AddScoped<IDeviceNamingService, DeviceNamingService>();

// Repositories
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();

// Application Command Services
builder.Services.AddScoped<DeviceCommandService>();
builder.Services.AddScoped<ZoneCommandService>();

// Application Query Services
builder.Services.AddScoped<DeviceQueryService>();
builder.Services.AddScoped<ZoneQueryService>();

var app = builder.Build();

// Verificar y crear base de datos si no existe (con manejo de errores)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Intentando conectar a la base de datos...");
        context.Database.EnsureCreated();
        logger.LogInformation("Base de datos conectada exitosamente.");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "No se pudo conectar a la base de datos. La aplicación continuará ejecutándose, pero las operaciones de base de datos fallarán.");
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
});

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();