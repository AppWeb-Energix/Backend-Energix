using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

namespace Energix.API.Controllers.Auth
{
    [ApiController]
    [Route("api/v1/auth")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Email y password requeridos" });

            var email = model.Email.Trim().ToLowerInvariant();

            var connString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connString))
                return StatusCode(500, new { error = "Cadena de conexión no configurada" });

            const string sql = @"
                SELECT id, email, username, password_hash
                FROM Users
                WHERE LOWER(email) = @email
                LIMIT 1;
            ";

            try
            {
                using var conn = new MySqlConnection(connString);
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new MySqlParameter("@email", MySqlDbType.VarChar) { Value = email });

                using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
                if (!await reader.ReadAsync())
                    return Unauthorized(new { error = "Usuario no encontrado, registrate primero" });

                var id = reader["id"]?.ToString();
                var dbEmail = reader["email"]?.ToString();
                var username = reader["username"]?.ToString();
                var storedHash = reader["password_hash"]?.ToString();

                if (string.IsNullOrEmpty(storedHash))
                    return Unauthorized(new { error = "Credenciales inválidas" });

                bool verified;
                try
                {
                    // Acepta ambos formatos:
                    // - Si el valor en BD parece un hash bcrypt (comienza con $2), verifica con BCrypt.
                    // - En caso contrario compara texto plano (SOLO PARA PRUEBAS LOCALES).
                    if (storedHash.StartsWith("$2"))
                    {
                        verified = BCrypt.Net.BCrypt.Verify(model.Password, storedHash);
                    }
                    else
                    {
                        // Comparación en texto plano (temporal e inseguro)
                        verified = string.Equals(model.Password, storedHash, StringComparison.Ordinal);
                    }
                }
                catch (Exception)
                {
                    return StatusCode(500, new { error = "Error al verificar contraseña" });
                }

                if (!verified)
                    return Unauthorized(new { error = "Credenciales inválidas" });

                // Generar token JWT usando la sección "Jwt" de appsettings
                var jwtSection = _configuration.GetSection("Jwt");
                var key = jwtSection["Key"];
                var issuer = jwtSection["Issuer"] ?? "energix";
                var audience = jwtSection["Audience"] ?? "energix-client";
                var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 60;

                if (string.IsNullOrEmpty(key))
                    return StatusCode(500, new { error = "Jwt:Key no configurada" });

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, id ?? ""),
                    new Claim(JwtRegisteredClaimNames.Email, dbEmail ?? ""),
                    new Claim("username", username ?? "")
                };

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expiresMinutes),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = creds
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);

                var userDto = new
                {
                    id,
                    email = dbEmail,
                    username
                };

                return Ok(new { token, user = userDto });
            }
            catch (Exception ex)
            {
                // En Development puede ser útil ver el detalle; en producción limitar la información.
                return Problem(detail: ex.ToString(), title: "Error al procesar login");
            }
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}