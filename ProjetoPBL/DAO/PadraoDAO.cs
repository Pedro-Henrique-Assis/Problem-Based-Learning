using ProjetoPBL.Models;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System;

namespace ProjetoPBL.DAO
{
    public abstract class PadraoDAO<T> where T : PadraoViewModel
    {
        public PadraoDAO()
        {
            SetTabela();
        }

        protected string Tabela { get; set; }
        protected string NomeSpListagem { get; set; } = "spListagem";
        public string NomeSpInsert { get; set; } = "";
        public string NomeSpUpdate { get; set; } = "";

        protected abstract SqlParameter[] CriaParametros(T model);
        protected abstract T MontaModel(DataRow registro);
        protected abstract void SetTabela();

        /// <summary>
        /// Insere um novo registro no banco de dados executando uma stored procedure.
        /// O nome da procedure é definido pela propriedade 'NomeSpInsert' ou, por padrão,
        /// construído como 'spInsert_{Tabela}'.
        /// </summary>
        /// <param name="model">O objeto ViewModel com os dados para criar os parâmetros da procedure.</param>
        public virtual void Insert(T model)
        {
            string nomeProc = string.IsNullOrEmpty(NomeSpInsert) ? "spInsert_" + Tabela : NomeSpInsert;
            HelperDAO.ExecutaProc(nomeProc, CriaParametros(model));
        }


        /// <summary>
        /// Atualiza um registro existente no banco de dados executando uma stored procedure.
        /// O nome da procedure é definido pela propriedade 'NomeSpUpdate' ou, por padrão,
        /// construído como 'spUpdate_{Tabela}'.
        /// </summary>
        /// <param name="model">O objeto ViewModel com os dados para criar os parâmetros da procedure.</param>
        public virtual void Update(T model)
        {
            string nomeProc = string.IsNullOrEmpty(NomeSpUpdate) ? "spUpdate_" + Tabela : NomeSpUpdate;
            HelperDAO.ExecutaProc(nomeProc, CriaParametros(model));
        }


        /// <summary>
        /// Exclui um registro do banco de dados executando a stored procedure genérica 'spDelete',
        /// passando o ID do registro e o nome da tabela como parâmetros.
        /// </summary>
        /// <param name="id">O ID do registro a ser excluído.</param>
        public virtual void Delete(int id)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("id", id),
                new SqlParameter("tabela", Tabela)
            };
            HelperDAO.ExecutaProc("spDelete", p);
        }


        /// <summary>
        /// Consulta e retorna um registro do banco de dados pelo seu ID,
        /// executando a stored procedure genérica 'spConsulta'.
        /// </summary>
        /// <param name="id">O ID do registro a ser consultado.</param>
        /// <returns>Um objeto ViewModel do tipo T com os dados do registro, ou null se não for encontrado.</returns>
        public virtual T Consulta(int id)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("id", id),
                new SqlParameter("tabela", Tabela)
            };
            var tabela = HelperDAO.ExecutaProcSelect("spConsulta", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaModel(tabela.Rows[0]);
        }


        /// <summary>
        /// Obtém o próximo ID disponível para uma tabela, executando a stored procedure 'spProximoId'.
        /// </summary>
        /// <returns>O próximo ID vago como um inteiro.</returns>
        public virtual int ProximoId()
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("tabela", Tabela)
            };
            var tabela = HelperDAO.ExecutaProcSelect("spProximoId", p);
            return Convert.ToInt32(tabela.Rows[0][0]);
        }


        /// <summary>
        /// Retorna uma lista com todos os registros da tabela,
        /// executando a stored procedure definida na propriedade 'NomeSpListagem'.
        /// </summary>
        /// <returns>Uma lista de objetos ViewModel do tipo T, preenchida com os dados da tabela.</returns>
        public virtual List<T> Listagem()
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("tabela", Tabela),
            };
            var tabela = HelperDAO.ExecutaProcSelect(NomeSpListagem, p);
            List<T> lista = new List<T>();
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaModel(registro));

            return lista;
        }
    }
}