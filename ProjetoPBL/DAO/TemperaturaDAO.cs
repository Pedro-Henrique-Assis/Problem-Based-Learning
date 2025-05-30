using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;

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
    }
}
