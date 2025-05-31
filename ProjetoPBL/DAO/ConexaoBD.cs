using System.Data.SqlClient;

namespace ProjetoPBL.DAO
{
    public static class ConexaoBD
    {
        /// <summary>
        /// Método estático que retorna um conexao aberta com o BD
        /// </summary>
        /// <returns>Conexão aberta</returns>
        public static SqlConnection GetConexao()
        {
            string strCon = "Data Source=LOCALHOST; Database=projeto_pbl; user id=sa; password=123456";
            SqlConnection conexao = new SqlConnection(strCon);
            conexao.Open();
            return conexao;
        }
    }
}


