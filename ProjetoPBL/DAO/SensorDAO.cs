using ProjetoPBL.Models;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoPBL.DAO
{
    /// <summary>
    /// DAO responsável pelo acesso à tabela de sensores
    /// </summary>
    public class SensorDAO : PadraoDAO<SensorViewModel>
    {
        /// <summary>
        /// Define a tabela e a stored procedure de listagem
        /// </summary>
        protected override void SetTabela()
        {
            Tabela = "sensor";
            NomeSpListagem = "spListagem";
            NomeSpInsert = "spInsert_sensor";
            NomeSpUpdate = "spUpdate_sensor";

        }

        /// <summary>
        /// Cria os parâmetros de SQL para inserir ou atualizar um sensor
        /// </summary>
        /// <param name="sensor">Objeto sensor</param>
        /// <returns>Array de parâmetros SQL</returns>
        protected override SqlParameter[] CriaParametros(SensorViewModel sensor)
        {
            List<SqlParameter> parametros = new List<SqlParameter>();

            if (sensor.Id != 0)
                parametros.Add(new SqlParameter("id", sensor.Id));

            parametros.Add(new SqlParameter("nomeSensor", sensor.nomeSensor));
            parametros.Add(new SqlParameter("descricaoSensor", sensor.descricaoSensor));
            parametros.Add(new SqlParameter("localInstalacao", sensor.localInstalacao));
            parametros.Add(new SqlParameter("valorInstalacao", sensor.valorInstalacao));
            parametros.Add(new SqlParameter("dataInstalacao", sensor.dataInstalacao));

            return parametros.ToArray();
        }

        /// <summary>
        /// Constrói um objeto SensorViewModel a partir de um DataRow
        /// </summary>
        /// <param name="registro">Linha de dados da tabela</param>
        /// <returns>Objeto SensorViewModel</returns>
        protected override SensorViewModel MontaModel(DataRow registro)
        {
            return new SensorViewModel
            {
                Id = Convert.ToInt32(registro["id"]),
                nomeSensor = registro["nomeSensor"].ToString(),
                descricaoSensor = registro["descricaoSensor"].ToString(),
                localInstalacao = registro["localInstalacao"].ToString(),
                valorInstalacao = Convert.ToDecimal(registro["valorInstalacao"]),
                dataInstalacao = Convert.ToDateTime(registro["dataInstalacao"])
            };
        }


        /// <summary>
        /// Verifica se já existe um sensor com o mesmo nome
        /// </summary>
        /// <param name="nomeSensor">Nome do sensor</param>
        /// <returns>Quantidade de sensores com o mesmo nome</returns>
        public int VerificarSensoresRepetidos(string nomeSensor)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("nomeSensor", nomeSensor) };
            DataTable dt = HelperDAO.ExecutaProcSelect("sp_verificar_sensor", sp);
            return Convert.ToInt32(dt.Rows[0]["cont"]);
        }

        public List<SensorViewModel> ConsultaAvancada(string local, decimal? valorMin, decimal? valorMax, DateTime? dataInicial, DateTime? dataFinal)
        {
            var lista = Listagem(); // ou um SELECT com WHERE

            if (!string.IsNullOrEmpty(local))
                lista = lista.Where(s => s.localInstalacao.Contains(local)).ToList();

            if (valorMin.HasValue)
                lista = lista.Where(s => s.valorInstalacao >= valorMin).ToList();

            if (valorMax.HasValue)
                lista = lista.Where(s => s.valorInstalacao <= valorMax).ToList();

            if (dataInicial.HasValue)
                lista = lista.Where(s => s.dataInstalacao >= dataInicial).ToList();

            if (dataFinal.HasValue)
                lista = lista.Where(s => s.dataInstalacao <= dataFinal).ToList();

            return lista;
        }

    }
}


