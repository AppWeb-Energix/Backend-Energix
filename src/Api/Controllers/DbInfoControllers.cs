using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Energix.API;

namespace Energix.API.Controllers
{
    [ApiController]
    [Route("api/v1/dbinfo")]
    public class DbInfoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public DbInfoController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET /api/v1/dbinfo/tables
        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection")
                              ?? "server=localhost;port=3306;database=energix;user=root;password=Enca248248+-;TreatTinyAsBoolean=true";

                var tables = new List<string>();
                string databaseName = null;

                await using (var conn = new MySqlConnection(connStr))
                {
                    await conn.OpenAsync();

                    // Obtener el nombre de la base de datos activa
                    await using (var cmdDb = conn.CreateCommand())
                    {
                        cmdDb.CommandText = "SELECT DATABASE();";
                        var res = await cmdDb.ExecuteScalarAsync();
                        databaseName = res?.ToString();
                    }

                    // Obtener tablas del schema/BD actual
                    await using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE();";
                        await using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tables.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                return Ok(new { database = databaseName, tables });
            }
            catch (Exception ex)
            {
                // En desarrollo devolvemos la excepción para depurar rápido.
                return Problem(detail: ex.ToString(), statusCode: 500);
            }
        }
    }
}