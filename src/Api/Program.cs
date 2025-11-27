using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Energix.API;
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

// --------------------
// Base de datos (EF Core / MySQL)
// --------------------
// Get connection string from configuration (appsettings.json / environment)
var defaultConn = configuration.GetConnectionString("DefaultConnection")
                  ?? configuration["ConnectionStrings:DefaultConnection"]
                  ?? "server=localhost;port=3306;database=energix;user=root;password=3xp3ri3nciA*";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));

// --------------------
// CORS
// --------------------
const string DevCorsPolicy = "AllowDev";
builder.Services.AddCors(o =>
{
    o.AddPolicy(DevCorsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --------------------
// JWT Auth
// --------------------
var jwtSection = configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? configuration["Jwt:Key"] ?? "changeme_replace_with_strong_key";
var jwtIssuer = jwtSection["Issuer"] ?? configuration["Jwt:Issuer"] ?? "energix";
var jwtAudience = jwtSection["Audience"] ?? configuration["Jwt:Audience"] ?? "energix-client";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
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

// --------------------
// DI (Identity + Personalization)
// --------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddPersonalizationServices();

// --------------------
// Controllers + Swagger
// --------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Energix API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer {token}'"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    });
});

// Register other app services here if needed, e.g.:
// builder.Services.AddScoped<IUserService, UserService>();

// Registrar módulo de Subscriptions
builder.Services.AddSubscriptionsInfrastructure(configuration);

var app = builder.Build();

// --------------------
// Pipeline
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    // HSTS solo si sirves HTTPS
    // app.UseHsts();
}

// Evitar warning de HTTPS si no hay puerto configurado
var hasHttpsPort = app.Configuration["ASPNETCORE_URLS"]?.Contains("https://") == true;
if (hasHttpsPort)
{
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Energix API v1"));

app.UseRouting();
app.UseCors(DevCorsPolicy);
app.UseAuthentication();
app.UseUserContext();
app.UseAuthorization();

app.MapControllers();

// Endpoint raíz informativo
app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    env = app.Environment.EnvironmentName,
    time = DateTime.UtcNow
}));

// Health sencillo
app.MapGet("/health", () => Results.Ok("healthy"));

// Readiness (chequeo rápido DB)
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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        Console.WriteLine("✅ Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Ocurrió un error al migrar la base de datos.");
    }
}

app.Run();
