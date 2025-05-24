using ProjetoPBL.Models;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;

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
            Tabela = "sensor"; // nome da tabela no banco
            NomeSpListagem = "spListagem"; // se você tiver essa SP criada
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
    }
}


//--Script de criação da tabela sensor
//CREATE TABLE sensor (
//    id INT IDENTITY(1,1) PRIMARY KEY,
//    nomeSensor VARCHAR(100) NOT NULL,
//    descricaoSensor VARCHAR(255),
//    localInstalacao VARCHAR(100),
//    valorInstalacao DECIMAL(10, 2),
//    dataInstalacao DATETIME
//);

//--Verifica se já existe um sensor com o mesmo nome
//CREATE PROCEDURE sp_verificar_sensor
//    @nomeSensor VARCHAR(100)
//AS
//BEGIN
//    SELECT COUNT(*) AS cont
//    FROM sensor
//    WHERE nomeSensor = @nomeSensor;
//END

//-- Insere um novo sensor
//CREATE PROCEDURE sp_inserir_sensor
//    @nomeSensor VARCHAR(100),
//    @descricaoSensor VARCHAR(255),
//    @localInstalacao VARCHAR(100),
//    @valorInstalacao DECIMAL(10,2),
//    @dataInstalacao DATETIME
//AS
//BEGIN
//    INSERT INTO sensor (nomeSensor, descricaoSensor, localInstalacao, valorInstalacao, dataInstalacao)
//    VALUES (@nomeSensor, @descricaoSensor, @localInstalacao, @valorInstalacao, @dataInstalacao);
//END

//-- Atualiza um sensor existente
//CREATE PROCEDURE sp_alterar_sensor
//    @id INT,
//    @nomeSensor VARCHAR(100),
//    @descricaoSensor VARCHAR(255),
//    @localInstalacao VARCHAR(100),
//    @valorInstalacao DECIMAL(10,2),
//    @dataInstalacao DATETIME
//AS
//BEGIN
//    UPDATE sensor
//    SET
//        nomeSensor = @nomeSensor,
//        descricaoSensor = @descricaoSensor,
//        localInstalacao = @localInstalacao,
//        valorInstalacao = @valorInstalacao,
//        dataInstalacao = @dataInstalacao
//    WHERE id = @id;
//END

