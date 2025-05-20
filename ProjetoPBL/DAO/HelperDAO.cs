using System.Data.SqlClient;
using System.Data;
using System;

namespace ProjetoPBL.DAO
{
    static class HelperDAO
    {
        /// <summary>
        /// Executa um comando SQL no banco de dados
        /// </summary>
        /// <param name="sql">Query</param>
        /// <param name="p">Parâmetros usados na query</param>
        public static void ExecutaSQL(string sql, SqlParameter[] p)
        {
            SqlConnection conexao = ConexaoBD.GetConexao();
            try
            {
                SqlCommand comando = new SqlCommand(sql, conexao);
                if (p != null)
                    comando.Parameters.AddRange(p);
                comando.ExecuteNonQuery();
            }
            finally
            {
                conexao.Close();
            }
        }

        /// <summary>
        /// Executa um comando select no banco de dados
        /// </summary>
        /// <param name="sql">Query</param>
        /// <param name="p">Parâmetros usados na query</param>
        /// <returns>Tabela onde cada DataRow é um registro do select</returns>
        public static DataTable ExecutaSelect(string sql, SqlParameter[] p)
        {
            SqlConnection cx = ConexaoBD.GetConexao();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(sql, cx);
                if (p != null)
                    adapter.SelectCommand.Parameters.AddRange(p);
                DataTable tabela = new DataTable();
                adapter.Fill(tabela);
                return tabela;
            }
            finally
            {
                cx.Close();
            }
        }

        /// <summary>
        /// Transforma o tipo null do .Net em DBNull.Value (compatível com o banco de dados)
        /// </summary>
        /// <param name="valor">Objeto que será verificado</param>
        /// <returns></returns>
        public static object NullAsDbNull(object valor)
        {
            if (valor == null)
                return DBNull.Value;
            else
                return valor;
        }


        /// <summary>
        /// Executa uma procedure específica no banco de dados
        /// </summary>
        /// <param name="nomeProc">Nome da stored procedure</param>
        /// <param name="parametros">Parâmetros usados na procedure</param>
        /// <returns>Retorna o resultado da procedure</returns>
        public static void ExecutaProc(string nomeProc, SqlParameter[] parametros)
        {
            using (SqlConnection conexao = ConexaoBD.GetConexao())
            {
                using (SqlCommand comando = new SqlCommand(nomeProc, conexao))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    if (parametros != null)
                        comando.Parameters.AddRange(parametros);
                    comando.ExecuteNonQuery();
                }
            }
        }


        public static DataTable ExecutaProcSelectX(string nomeProc, SqlParameter parametro)
        {
            SqlParameter[] p =
            {
                parametro
            };
            return ExecutaProcSelect(nomeProc, p);
        }

        /// <summary>
        /// Executa uma procedure que contém um comando select
        /// </summary>
        /// <param name="nomeProc">Nome da stored procedure</param>
        /// <param name="parametros">Parâmetros usados na procedure</param>
        /// <returns></returns>
        public static DataTable ExecutaProcSelect(string nomeProc, SqlParameter[] parametros)
        {
            using (SqlConnection conexao = ConexaoBD.GetConexao())
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(nomeProc, conexao))
                {
                    if (parametros != null)
                        adapter.SelectCommand.Parameters.AddRange(parametros);
                    adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    DataTable tabela = new DataTable();
                    adapter.Fill(tabela);
                    return tabela;
                }
            }
        }
    }
}
