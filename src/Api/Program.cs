using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Energix.API;
using Energix.Subscriptions.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configuration quick shortcuts
var configuration = builder.Configuration;

// --------------------
// Database (EF Core) - MySQL
// --------------------
// Get connection string from configuration (appsettings.json / environment)
var defaultConn = configuration.GetConnectionString("DefaultConnection")
                  ?? configuration["ConnectionStrings:DefaultConnection"]
                  ?? "server=localhost;port=3306;database=energix;user=root;password=your_password";

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

// Register other app services here if needed, e.g.:
// builder.Services.AddScoped<IUserService, UserService>();

// Registrar módulo de Subscriptions
builder.Services.AddSubscriptionsInfrastructure(configuration);

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
app.UseAuthorization();

// Map controllers (tu carpeta controllers/auth con LoginController y RegisterController será detectada)
app.MapControllers();

// Optional health/readiness endpoints
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();