using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Energix.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET /api/v1/users?email=...
        [HttpGet]
        public async Task<IActionResult> GetByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { error = "El parámetro 'email' es obligatorio." });

            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection")
                              ?? "server=localhost;port=3306;database=energix;user=root;password=Enca248248+-;TreatTinyAsBoolean=true";

                await using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                // Ajustado a la tabla Users que tienes. Cambia columnas si tu esquema difiere.
                cmd.CommandText = "SELECT Id, Email, UserName FROM `Users` WHERE Email = @email LIMIT 1;";
                cmd.Parameters.AddWithValue("@email", email);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return Ok(new
                    {
                        id = reader["Id"]?.ToString(),
                        email = reader["Email"]?.ToString(),
                        username = reader["UserName"]?.ToString()
                    });
                }

                return NotFound(new { error = "Usuario no encontrado." });
            }
            catch (Exception ex)
            {
                // En Development devolvemos la excepción para depurar rápido.
                return Problem(detail: ex.ToString(), statusCode: 500);
            }
        }
    }
}