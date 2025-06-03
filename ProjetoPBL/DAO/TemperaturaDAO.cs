using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

namespace ProjetoPBL.DAO
{
    public class TemperaturaDAO
    {
        /// <summary>
        /// String de conexão com o banco de dados SQL Server local.
        /// </summary>
        private readonly string connectionString = @"Data Source=localhost;Initial Catalog=projeto_pbl;user id=sa;password=123456;";

        /// <summary>
        /// Verifica se já existe um registro de temperatura para um dado sensor, data e valor de temperatura.
        /// </summary>
        /// <param name="recvTime">Data e hora do registro de temperatura.</param>
        /// <param name="temperature">Valor da temperatura registrada.</param>
        /// <param name="sensorId">Identificador do sensor.</param>
        /// <returns>Retorna true se o registro já existir no banco; caso contrário, false.</returns>
        public bool ExisteRegistro(DateTime recvTime, float temperature, string sensorId)
        {
            using var conexao = new SqlConnection(connectionString);

            var parametros = new DynamicParameters();
            parametros.Add("@SensorId", sensorId, DbType.String);
            parametros.Add("@RecvTime", recvTime, DbType.DateTime);
            parametros.Add("@Temperature", temperature, DbType.Double);

            int count = conexao.ExecuteScalar<int>("spExisteRegistro", parametros, commandType: CommandType.StoredProcedure);
            return count > 0;
        }

        /// <summary>
        /// Insere um novo registro de temperatura caso ele ainda não exista no banco de dados.
        /// </summary>
        /// <param name="temp">Objeto contendo os dados de temperatura a ser inserido.</param>
        public void InserirSeNaoExiste(TemperaturaViewModel temp)
        {
            if (!ExisteRegistro(temp.RecvTime, temp.Temperature, temp.SensorId))
            {
                Inserir(temp);
            }
        }

        /// <summary>
        /// Insere um registro de temperatura no banco de dados.
        /// </summary>
        /// <param name="temp">Objeto contendo os dados de temperatura a ser inserido.</param>
        public void Inserir(TemperaturaViewModel temp)
        {
            using var conexao = new SqlConnection(connectionString);

            var parametros = new DynamicParameters();
            parametros.Add("@SensorId", temp.SensorId, DbType.String);
            parametros.Add("@RecvTime", temp.RecvTime, DbType.DateTime);
            parametros.Add("@Temperature", temp.Temperature, DbType.Double);

            conexao.Execute("spInserirTemperatura", parametros, commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Recupera a lista completa de registros de temperatura do banco de dados.
        /// </summary>
        /// <returns>Lista de objetos <see cref="TemperaturaViewModel"/> representando os registros de temperatura.</returns>
        public List<TemperaturaViewModel> Listar()
        {
            using var conexao = new SqlConnection(connectionString);

            return conexao.Query<TemperaturaViewModel>("spListarTemperaturas", commandType: CommandType.StoredProcedure).AsList();
        }

        /// <summary>
        /// Calcula parâmetros importantes da resposta do sistema, incluindo ganho estático (K), constante de tempo (tau),
        /// o tempo real em que o sistema atingiu o valor de referência, e o valor alvo de 63,2% do ganho.
        /// </summary>
        /// <returns>
        /// Uma tupla contendo:
        /// <list type="bullet">
        /// <item>ganhoK: Valor do ganho estático do sistema.</item>
        /// <item>constanteTempoTau: Tempo em segundos correspondente à constante de tempo (tau).</item>
        /// <item>tempoRealTau: Data e hora em que o sistema atingiu o valor de referência.</item>
        /// <item>alvo632: Valor alvo de 63,2% do ganho estático.</item>
        /// </list>
        /// </returns>
        public (float ganhoK, double constanteTempoTau, DateTime? tempoRealTau, float alvo632) CalcularParametros()
        {
            var dados = Listar();

            if (dados == null || dados.Count < 2)
                return (0, 0, null, 0);

            var dadosOrdenados = dados.OrderBy(t => t.RecvTime).ToList();

            float tempInicial = dadosOrdenados.First().Temperature;

            // Média das últimas 10 temperaturas registradas para estimar temperatura final estável
            var ultimos = dadosOrdenados.TakeLast(10).Select(d => d.Temperature);
            float tempFinal = ultimos.Average();

            float ganhoK = tempFinal - tempInicial;
            float alvo = tempInicial + 0.632f * ganhoK; // Alvo de 63,2% do ganho (constante de tempo do sistema)
            DateTime tempoInicial = dadosOrdenados.First().RecvTime;

            // Busca o momento em que a temperatura atinge ou ultrapassa o valor alvo
            foreach (var ponto in dadosOrdenados)
            {
                if (ponto.Temperature >= alvo)
                {
                    double tempoTau = (ponto.RecvTime - tempoInicial).TotalSeconds;
                    return (ganhoK, tempoTau, ponto.RecvTime, alvo);
                }
            }

            // Caso o valor alvo não seja atingido, retorna ganho, zero para tempo e o valor do alvo
            return (ganhoK, 0, null, alvo);
        }
    }
}
