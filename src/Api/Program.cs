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

var builder = WebApplication.CreateBuilder(args);

// Configuration quick shortcuts
var configuration = builder.Configuration;

// --------------------
// Database (EF Core) - MySQL
// --------------------
// Get connection string from configuration (appsettings.json / environment)
var defaultConn = configuration.GetConnectionString("DefaultConnection")
                  ?? configuration["ConnectionStrings:DefaultConnection"]
                  ?? "server=localhost;port=3306;database=energix;user=root;password=3xp3ri3nciA*";

// Use Pomelo or MySql provider. ServerVersion.AutoDetect will try to detect the server version.
// Make sure the provider package (Pomelo.EntityFrameworkCore.MySql) is installed in the API project.
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
        // En desarrollo permitir todo; en producción restringe a los orígenes necesarios.
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --------------------
// Authentication - JWT Bearer
// --------------------
// Configuration expects a section "Jwt" with "Key", "Issuer" and "Audience".
// Example appsettings.json:
// "Jwt": { "Key": "super-secret-key-change-me", "Issuer": "energix", "Audience": "energix-client", "ExpiresMinutes": "60" }
var jwtSection = configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_KEY") ?? "changeme_replace_with_strong_key";
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
        options.RequireHttpsMetadata = false; // en producción true + HTTPS
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

// --------------------
// Identity Bounded Context - Dependency Injection
// --------------------
// Configure TokenSettings from appsettings "Jwt" section
builder.Services.Configure<TokenSettings>(configuration.GetSection("Jwt"));

// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register domain services
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Register application services
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();

// --------------------
// Controllers, Swagger, other services
// --------------------
builder.Services.AddControllers();

// Swagger / OpenAPI with Bearer auth UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Energix API", Version = "v1" });

    // JWT Authorization in Swagger
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


var app = builder.Build();

// --------------------
// Middleware pipeline
// --------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Energix API v1"));
}
else
{
    app.UseExceptionHandler("/error"); // o custom handler
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(DevCorsPolicy);

app.UseAuthentication();
app.UseUserContext(); // Loads authenticated user into HttpContext.Items
app.UseAuthorization();

// Map controllers (tu carpeta controllers/auth con LoginController y RegisterController será detectada)
app.MapControllers();

// Optional health/readiness endpoints
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();