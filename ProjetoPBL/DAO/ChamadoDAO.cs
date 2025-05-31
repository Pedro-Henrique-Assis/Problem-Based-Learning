using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ProjetoPBL.Models;

namespace ProjetoPBL.DAO
{
    public class ChamadoDAO : PadraoDAO<ChamadoViewModel>
    {
        protected override void SetTabela()
        {
            Tabela = "chamados";
            NomeSpListagem = "dbo.spListagemChamados";
            NomeSpInsert = "dbo.spInsertChamados";
            NomeSpUpdate = "dbo.spUpdateChamados";
        }

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
                new SqlParameter("@resposta", (object)model.Resposta ?? DBNull.Value)
            };
        }

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

        // NOVO MÉTODO: usa a stored procedure spConsultaChamados com permissões
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

        // Listar todos os chamados (usando spListagemChamados)
        public override List<ChamadoViewModel> Listagem()
        {
            var tabela = HelperDAO.ExecutaProcSelect(NomeSpListagem, null);
            List<ChamadoViewModel> lista = new List<ChamadoViewModel>();
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaModel(registro));
            return lista;
        }

        // Consulta por ID individual (não usa a nova SP)
        public override ChamadoViewModel Consulta(int id)
        {
            var p = new SqlParameter[] { new SqlParameter("@id", id) };
            var tabela = HelperDAO.ExecutaProcSelect("dbo.spConsultaChamadoPorId", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaModel(tabela.Rows[0]);
        }

        // Insert que retorna o ID do chamado criado
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

                    var novoId = Convert.ToInt32(comando.ExecuteScalar());
                    return novoId;
                }
            }
        }

        // Update
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

        // Delete
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

        // Responder chamado
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



//TESTE GIT CAIO BURRO
