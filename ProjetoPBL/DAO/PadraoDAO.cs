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

        // NOVAS propriedades opcionais
        public string NomeSpInsert { get; set; } = "";
        public string NomeSpUpdate { get; set; } = "";

        protected abstract SqlParameter[] CriaParametros(T model);
        protected abstract T MontaModel(DataRow registro);
        protected abstract void SetTabela();

        /// <summary>
        /// Executa uma stored procedure que insere
        /// um registro no banco de dados
        /// </summary>
        public virtual void Insert(T model)
        {
            string nomeProc = string.IsNullOrEmpty(NomeSpInsert) ? "spInsert_" + Tabela : NomeSpInsert;
            HelperDAO.ExecutaProc(nomeProc, CriaParametros(model));
        }

        public virtual void Update(T model)
        {
            string nomeProc = string.IsNullOrEmpty(NomeSpUpdate) ? "spUpdate_" + Tabela : NomeSpUpdate;
            HelperDAO.ExecutaProc(nomeProc, CriaParametros(model));
        }

        public virtual void Delete(int id)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("id", id),
                new SqlParameter("tabela", Tabela)
            };
            HelperDAO.ExecutaProc("spDelete", p);
        }

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

        public virtual int ProximoId()
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("tabela", Tabela)
            };
            var tabela = HelperDAO.ExecutaProcSelect("spProximoId", p);
            return Convert.ToInt32(tabela.Rows[0][0]);
        }

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

//Teste Ana