using ProjetoPBL.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ProjetoPBL.DAO
{
    public class SexoDAO : PadraoDAO<SexoViewModel>
    {
        /// <summary>
        /// Cria e retorna um array de SqlParameters a partir de um objeto SexoViewModel.
        /// Este método é usado para passar os dados do modelo para as stored procedures.
        /// </summary>
        /// <param name="model">O SexoViewModel contendo os dados de 'id' e 'nome'.</param>
        /// <returns>Array de SqlParameters para a execução da procedure.</returns>
        protected override SqlParameter[] CriaParametros(SexoViewModel model)
        {
            SqlParameter[] parametros =
            {
                 new SqlParameter("id", model.Id),
                 new SqlParameter("nome", model.Nome)
            };
            return parametros;
        }


        /// <summary>
        /// Converte um DataRow de uma consulta ao banco em um objeto SexoViewModel.
        /// </summary>
        /// <param name="registro">O DataRow contendo os dados de um registro de sexo.</param>
        /// <returns>Um objeto SexoViewModel preenchido com os dados do DataRow.</returns>
        protected override SexoViewModel MontaModel(DataRow registro)
        {
            var s = new SexoViewModel()
            {
                Id = Convert.ToInt32(registro["id"]),
                Nome = registro["nome"].ToString()
            };
            return s;
        }

        protected override void SetTabela()
        {
            Tabela = "sexos";
        }
    }
}