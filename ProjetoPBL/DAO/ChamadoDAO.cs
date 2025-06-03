using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ProjetoPBL.Models;

namespace ProjetoPBL.DAO
{
    // DAO específico para operações CRUD e consultas da entidade Chamado
    public class ChamadoDAO : PadraoDAO<ChamadoViewModel>
    {
        // Define as configurações básicas do DAO: tabela e nomes das stored procedures usadas
        protected override void SetTabela()
        {
            Tabela = "chamados";
            NomeSpListagem = "dbo.spListagemChamados";
            NomeSpInsert = "dbo.spInsertChamados";
            NomeSpUpdate = "dbo.spUpdateChamados";
        }

        // Cria os parâmetros SQL para inserção/atualização a partir do model recebido
        protected override SqlParameter[] CriaParametros(ChamadoViewModel model)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@id", model.Id),
                new SqlParameter("@titulo", model.Titulo),
                new SqlParameter("@descricao", model.Descricao),
                new SqlParameter("@status", model.Status),
                new SqlParameter("@data_abertura", model.DataAbertura),
                new SqlParameter("@usuario_id", model.UsuarioId),
                // Se resposta for null, envia DBNull para o parâmetro SQL
                new SqlParameter("@resposta", (object)model.Resposta ?? DBNull.Value)
            };
        }

        // Monta um objeto ChamadoViewModel a partir de uma linha do DataTable
        protected override ChamadoViewModel MontaModel(DataRow registro)
        {
            return new ChamadoViewModel
            {
                Id = Convert.ToInt32(registro["id"]),
                Titulo = registro["titulo"].ToString(),
                Descricao = registro["descricao"].ToString(),
                Status = registro["status"].ToString(),
                DataAbertura = Convert.ToDateTime(registro["data_abertura"]),
                UsuarioId = Convert.ToInt32(registro["usuario_id"]),
                Resposta = registro["resposta"] == DBNull.Value ? null : registro["resposta"].ToString()
            };
        }

        /// <summary>
        /// Consulta chamados aplicando permissões, usando stored procedure que considera se o usuário é admin
        /// </summary>
        /// <param name="usuarioId">ID do usuário logado</param>
        /// <param name="isAdmin">Flag que indica se o usuário é admin</param>
        /// <returns>Lista de chamados filtrada conforme permissão</returns>
        public List<ChamadoViewModel> ConsultaChamadosPorPermissao(int usuarioId, bool isAdmin)
        {
            var parametros = new SqlParameter[]
            {
                new SqlParameter("@usuario_id", usuarioId),
                new SqlParameter("@is_admin", isAdmin)
            };

            var tabela = HelperDAO.ExecutaProcSelect("dbo.spConsultaChamados", parametros);

            List<ChamadoViewModel> lista = new List<ChamadoViewModel>();
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaModel(registro));

            return lista;
        }

        /// <summary>
        /// Lista todos os chamados usando a stored procedure padrão de listagem
        /// </summary>
        /// <returns>Lista completa de chamados</returns>
        public override List<ChamadoViewModel> Listagem()
        {
            var tabela = HelperDAO.ExecutaProcSelect(NomeSpListagem, null);
            List<ChamadoViewModel> lista = new List<ChamadoViewModel>();
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaModel(registro));
            return lista;
        }

        /// <summary>
        /// Consulta um chamado pelo seu ID
        /// </summary>
        /// <param name="id">ID do chamado</param>
        /// <returns>Objeto ChamadoViewModel ou null se não encontrado</returns>
        public override ChamadoViewModel Consulta(int id)
        {
            var p = new SqlParameter[] { new SqlParameter("@id", id) };
            var tabela = HelperDAO.ExecutaProcSelect("dbo.spConsultaChamadoPorId", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaModel(tabela.Rows[0]);
        }

        /// <summary>
        /// Insere um novo chamado e retorna o ID gerado
        /// </summary>
        /// <param name="model">Dados do chamado a inserir</param>
        /// <returns>ID do chamado recém-criado</returns>
        public int InsertRetornandoId(ChamadoViewModel model)
        {
            using (var conexao = ConexaoBD.GetConexao())
            {
                using (var comando = new SqlCommand("dbo.spInsertChamados", conexao))
                {
                    comando.CommandType = CommandType.StoredProcedure;

                    comando.Parameters.AddWithValue("@titulo", model.Titulo);
                    comando.Parameters.AddWithValue("@descricao", model.Descricao);
                    comando.Parameters.AddWithValue("@status", model.Status);
                    comando.Parameters.AddWithValue("@data_abertura", model.DataAbertura);
                    comando.Parameters.AddWithValue("@usuario_id", model.UsuarioId);
                    comando.Parameters.AddWithValue("@resposta", (object)model.Resposta ?? DBNull.Value);

                    // ExecuteScalar retorna o primeiro valor da primeira linha, usado aqui para pegar o ID
                    var novoId = Convert.ToInt32(comando.ExecuteScalar());
                    return novoId;
                }
            }
        }

        /// <summary>
        /// Atualiza os dados de um chamado existente
        /// </summary>
        /// <param name="model">Objeto com os dados atualizados</param>
        public override void Update(ChamadoViewModel model)
        {
            using (var conexao = ConexaoBD.GetConexao())
            {
                using (var comando = new SqlCommand("dbo.spUpdateChamados", conexao))
                {
                    comando.CommandType = CommandType.StoredProcedure;

                    comando.Parameters.AddWithValue("@id", model.Id);
                    comando.Parameters.AddWithValue("@titulo", model.Titulo);
                    comando.Parameters.AddWithValue("@descricao", model.Descricao);
                    comando.Parameters.AddWithValue("@status", model.Status);
                    comando.Parameters.AddWithValue("@data_abertura", model.DataAbertura);
                    comando.Parameters.AddWithValue("@usuario_id", model.UsuarioId);
                    comando.Parameters.AddWithValue("@resposta", (object)model.Resposta ?? DBNull.Value);

                    comando.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Remove um chamado do banco pelo seu ID
        /// </summary>
        /// <param name="id">ID do chamado a ser deletado</param>
        public override void Delete(int id)
        {
            using (var conexao = ConexaoBD.GetConexao())
            {
                using (var comando = new SqlCommand("dbo.spDeleteChamados", conexao))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@id", id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Registra uma resposta e atualiza o status do chamado
        /// </summary>
        /// <param name="id">ID do chamado</param>
        /// <param name="resposta">Texto da resposta</param>
        /// <param name="status">Novo status do chamado</param>
        public void Responder(int id, string resposta, string status)
        {
            using (var conexao = ConexaoBD.GetConexao())
            {
                using (var comando = new SqlCommand("dbo.spResponderChamado", conexao))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@id", id);
                    comando.Parameters.AddWithValue("@resposta", resposta);
                    comando.Parameters.AddWithValue("@status", status);

                    comando.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Lista chamados pertencentes a um usuário específico, ordenados pela data de abertura decrescente
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <returns>Lista de chamados do usuário</returns>
        public List<ChamadoViewModel> ListarPorUsuario(int usuarioId)
        {
            string sql = "SELECT * FROM chamados WHERE usuario_id = @usuario_id ORDER BY data_abertura DESC";
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@usuario_id", usuarioId)
            };
            DataTable tabela = HelperDAO.ExecutaSelect(sql, parametros);

            List<ChamadoViewModel> lista = new List<ChamadoViewModel>();
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaModel(registro));

            return lista;
        }

        /// <summary>
        /// Lista todos os chamados ordenados pela data de abertura decrescente
        /// </summary>
        /// <returns>Lista completa de chamados</returns>
        public List<ChamadoViewModel> ListarTodos()
        {
            string sql = "SELECT * FROM chamados ORDER BY data_abertura DESC";
            DataTable tabela = HelperDAO.ExecutaSelect(sql, null);

            List<ChamadoViewModel> lista = new List<ChamadoViewModel>();
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaModel(registro));

            return lista;
        }
    }
}
