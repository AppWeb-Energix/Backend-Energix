// Program.cs
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Energix.API;
using Energix.API.DeviceManagement.Application.Internal.CommandServices;
using Energix.API.DeviceManagement.Application.Internal.QueryServices;
using Energix.API.DeviceManagement.Domain.Repositories;
using Energix.API.DeviceManagement.Domain.Services;
using Energix.API.DeviceManagement.Infrastructure.Persistence.EFC.Repositories;
using Energix.API.DeviceManagement.Infrastructure.Services;
using Energix.API.Identity.Application.Services;
using Energix.API.Identity.Domain.Repositories;
using Energix.API.Identity.Domain.Services;
using Energix.API.Identity.Infrastructure.Hashing;
using Energix.API.Identity.Infrastructure.Persistence.Repositories;
using Energix.API.Identity.Infrastructure.Tokens;
using Energix.API.Identity.Infrastructure.Authorization.Middleware;
using Energix.API.Personalization.Infrastructure;
using Energix.Subscriptions.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// DB (MySQL)
var defaultConn = configuration.GetConnectionString("DefaultConnection")
                  ?? configuration["ConnectionStrings:DefaultConnection"]
                  ?? "server=localhost;port=3306;database=energix;user=root;password=Password123";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));

// CORS
const string FrontendCorsPolicy = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        var origins = new[]
        {
            "https://frontend-energix.vercel.app",
            "https://backend-energix.onrender.com",
            "http://localhost:5173"
        };
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Si necesitas cookies/JWT en navegador: agregar .AllowCredentials()
    });
});

// JWT
var jwtSection = configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT_KEY no configurada");
var jwtIssuer = jwtSection["Issuer"] ?? configuration["Jwt:Issuer"] ?? "energix";
var jwtAudience = jwtSection["Audience"] ?? configuration["Jwt:Audience"] ?? "energix-client";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.SaveToken = true;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.Configure<TokenSettings>(configuration.GetSection("Jwt"));

// Identity + Personalization
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddPersonalizationServices();

// Device Management
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<IDeviceQueryService, DeviceQueryService>();
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();
builder.Services.AddScoped<IDeviceNamingService, DeviceNamingService>();
builder.Services.AddScoped<IPlanValidationService, PlanValidationService>();

// Subscriptions
builder.Services.AddSubscriptionsInfrastructure(configuration);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Energix API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT sin la palabra Bearer."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Energix API v1"));
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Energix API v1"));
    app.UseExceptionHandler("/error");
}

var hasHttpsPort = app.Configuration["ASPNETCORE_URLS"]?.Contains("https://") == true;
if (hasHttpsPort)
    app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseUserContext();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    env = app.Environment.EnvironmentName,
    time = DateTime.UtcNow
}));

app.MapGet("/health", () => Results.Ok("healthy"));
app.MapGet("/ready", async (AppDbContext db) =>
{
    try
    {
        await db.Database.ExecuteSqlRawAsync("SELECT 1");
        return Results.Ok("ready");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Migraciones
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ Migraciones aplicadas.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error al migrar la base de datos.");
    }
}

app.Run();
