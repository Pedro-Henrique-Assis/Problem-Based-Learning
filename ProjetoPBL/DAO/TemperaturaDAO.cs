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
        private readonly string connectionString = @"Data Source=localhost;Initial Catalog=projeto_pbl;user id=sa;password=123456;";

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

        public void InserirSeNaoExiste(TemperaturaViewModel temp)
        {
            if (!ExisteRegistro(temp.RecvTime, temp.Temperature, temp.SensorId))
            {
                Inserir(temp);
            }
        }

        public void Inserir(TemperaturaViewModel temp)
        {
            using var conexao = new SqlConnection(connectionString);
            var parametros = new DynamicParameters();
            parametros.Add("@SensorId", temp.SensorId, DbType.String);
            parametros.Add("@RecvTime", temp.RecvTime, DbType.DateTime);
            parametros.Add("@Temperature", temp.Temperature, DbType.Double);

            conexao.Execute("spInserirTemperatura", parametros, commandType: CommandType.StoredProcedure);
        }

        public List<TemperaturaViewModel> Listar()
        {
            using var conexao = new SqlConnection(connectionString);
            return conexao.Query<TemperaturaViewModel>("spListarTemperaturas", commandType: CommandType.StoredProcedure).AsList();
        }

        public (float ganhoK, double constanteTempoTau, DateTime? tempoRealTau, float alvo632) CalcularParametros()
        {
            var dados = Listar();

            if (dados == null || dados.Count < 2)
                return (0, 0, null, 0);

            var dadosOrdenados = dados.OrderBy(t => t.RecvTime).ToList();

            float tempInicial = dadosOrdenados.First().Temperature;
            var ultimos = dadosOrdenados.TakeLast(10).Select(d => d.Temperature);
            float tempFinal = ultimos.Average();

            float ganhoK = tempFinal - tempInicial;
            float alvo = tempInicial + 0.632f * ganhoK;
            DateTime tempoInicial = dadosOrdenados.First().RecvTime;

            foreach (var ponto in dadosOrdenados)
            {
                if (ponto.Temperature >= alvo)
                {
                    double tempoTau = (ponto.RecvTime - tempoInicial).TotalSeconds;
                    return (ganhoK, tempoTau, ponto.RecvTime, alvo);
                }
            }

            return (ganhoK, 0, null, alvo);
        }

    }
}
