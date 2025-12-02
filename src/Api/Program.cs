using System;
using System.IdentityModel.Tokens.Jwt;
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
                  ?? Environment.GetEnvironmentVariable("DB_CONNECTION")
                  ?? "server=localhost;port=3306;database=energix;user=root;password=3xp3ri3nciA*";

Console.WriteLine($"[STARTUP] Conectando a BD: {defaultConn.Replace("password=", "password=***")}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));

// CORS
const string FrontendCorsPolicy = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(
                "https://frontend-energix.vercel.app",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromHours(1));
    });
});

// JWT
var jwtSection = configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new InvalidOperationException("JWT_KEY no configurada");
var jwtIssuer = jwtSection["Issuer"] ?? configuration["Jwt:Issuer"] ?? "energix";
var jwtAudience = jwtSection["Audience"] ?? configuration["Jwt:Audience"] ?? "energix-client";

Console.WriteLine($"[JWT CONFIG] Issuer: {jwtIssuer}, Audience: {jwtAudience}");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(o =>
    {
        // ✅ Limpiar el mapeo de claims automático y fijar NameClaimType a "sub"
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        
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
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Sub // ✅ Configurar NameClaimType
        };

        // ✅ DEBUG: Logs de eventos JWT
        o.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ [JWT ERROR] {context.Exception.Message}");
                if (context.Exception is SecurityTokenExpiredException)
                {
                    Console.WriteLine("⏰ [JWT] Token expirado");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst("sub")?.Value;
                var email = context.Principal?.FindFirst("email")?.Value;
                Console.WriteLine($"✅ [JWT OK] Token válido - UserId: {userId}, Email: {email}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"⚠️ [JWT CHALLENGE] Error: {context.Error}, Descripción: {context.ErrorDescription}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine($"🔐 [JWT] Token recibido: {token.Substring(0, Math.Min(50, token.Length))}...");
                }
                else
                {
                    Console.WriteLine("⚠️ [JWT] No se recibió token Authorization");
                }
                return Task.CompletedTask;
            }
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

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Energix API v1"));

// HTTPS
var hasHttpsPort = app.Configuration["ASPNETCORE_URLS"]?.Contains("https://") == true;
if (hasHttpsPort)
    app.UseHttpsRedirection();

app.UseRouting();

// ✅ CORS primero
app.UseCors(FrontendCorsPolicy);

// ✅ Middleware simplificado para errores globales
app.Use(async (HttpContext ctx, RequestDelegate next) =>
{
    try
    {
        await next(ctx);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GLOBAL ERROR] {ex.GetType().Name}: {ex.Message}");
        Console.WriteLine($"[STACK] {ex.StackTrace}");

        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";

        if (!ctx.Response.HasStarted)
        {
            await ctx.Response.WriteAsJsonAsync(new
            {
                error = ex.Message,
                type = ex.GetType().Name,
                message = "Internal server error"
            });
        }
    }
});

// ⚠️ ORDEN CRÍTICO
app.UseAuthentication();  // 1️⃣ Autenticación
app.UseUserContext();     // 2️⃣ Contexto de usuario
app.UseAuthorization();   // 3️⃣ Autorización

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
        Console.WriteLine($"[DB CHECK FAILED] {ex.Message}");
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
        Console.WriteLine("[MIGRATION] Aplicando migraciones...");
        context.Database.Migrate();
        Console.WriteLine("[MIGRATION] Migraciones aplicadas exitosamente.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "[MIGRATION ERROR] Error al migrar la base de datos.");
        Console.WriteLine($"[MIGRATION ERROR] {ex.Message}");
    }
}

Console.WriteLine("[STARTUP] Aplicación iniciada correctamente.");
app.Run();
