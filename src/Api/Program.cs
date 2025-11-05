// csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI y explorador de endpoints
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Controllers
builder.Services.AddControllers();

// CORS: permite cualquier origen, método y cabecera
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// DbContext MySQL
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
              ?? "server=localhost;port=3306;database=energix;user=root;password=tu_password;TreatTinyAsBoolean=true";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connStr, ServerVersion.AutoDetect(connStr)));

var app = builder.Build();

// Soporte de cabeceras reenviadas para proxy o balanceador
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
    // Configura KnownProxies o KnownNetworks si es necesario
});

// Activa CORS
app.UseCors("AllowAll");

// Swagger solo en Development o protegido por API Key en otros entornos
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "API v1");
        c.RoutePrefix = "swagger"; // /swagger
    });
}
else
{
    // Protege Swagger con API Key si se define Swagger:ApiKey
    var swaggerKey = builder.Configuration["Swagger:ApiKey"];

    if (!string.IsNullOrEmpty(swaggerKey))
    {
        app.UseWhen(
            ctx => ctx.Request.Path.StartsWithSegments("/swagger") || ctx.Request.Path.StartsWithSegments("/openapi"),
            branch =>
            {
                branch.Use(async (ctx, next) =>
                {
                    if (!ctx.Request.Headers.TryGetValue("X-Swagger-Key", out var provided) || provided != swaggerKey)
                    {
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await ctx.Response.WriteAsync("Unauthorized");
                        return;
                    }
                    await next();
                });
            });

        app.MapOpenApi();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/openapi/v1.json", "API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
