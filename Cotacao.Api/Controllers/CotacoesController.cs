
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace APICotacoes.Controllers
{
 [ApiController]
    [Route("[controller]")]
    public class CotacoesController : ControllerBase
    {
        [HttpGet]
        public ContentResult Get(
            [FromServices]IConfiguration config,
            [FromServices]IDistributedCache cache)
        {
            string valorJSON = cache.GetString("Cotacoes");
            if (valorJSON == null)
            {
                string strConn = @"Data Source=C:\trabalho\projetosjuliano\diarios\arquitetura\NETCORE\NetCore_CacheRedis\Cotacao.Api\BaseCotacoes.db";
                SqliteConnection conn;
                using (conn = new SqliteConnection(strConn))
                {
                    conn.Open();
                }

                string CommandText =
                    "SELECT Sigla " +
                          ",NomeMoeda " +
                          ",UltimaCotacao " +
                          ",ValorComercial AS 'Cotacoes.Comercial' " +
                          ",ValorTurismo AS 'Cotacoes.Turismo' " +
                    "FROM dbo.Cotacoes " +
                    "ORDER BY NomeMoeda " +
                    "FOR JSON PATH, ROOT('Moedas')";

                using (var comm = new SqliteCommand(CommandText, conn))
                {
                    comm.CommandText = "DELETE FROM Cliente";
                    valorJSON = (string)comm.ExecuteScalar();
                }
                                
                DistributedCacheEntryOptions opcoesCache =
                          new DistributedCacheEntryOptions();
                opcoesCache.SetAbsoluteExpiration(
                    TimeSpan.FromMinutes(1));

                cache.SetString("Cotacoes", valorJSON, opcoesCache);
            }

            return Content(valorJSON, "application/json");
        }
    }
}