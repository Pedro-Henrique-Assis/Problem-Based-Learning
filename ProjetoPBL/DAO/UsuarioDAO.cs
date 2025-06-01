using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace ProjetoPBL.DAO
{
    public class UsuarioDAO : PadraoDAO<UsuarioViewModel>
    {
        protected override void SetTabela()
        {
            Tabela = "usuarios";
            NomeSpListagem = "spListagem";
        }

        /// <summary>
        /// Cria os parâmetros SQL para Insert e Update, incluindo IsAdmin.
        /// </summary>
        protected override SqlParameter[] CriaParametros(UsuarioViewModel usuario)
        {
			List<SqlParameter> parametros = new List<SqlParameter>
			{
				new SqlParameter("@id", usuario.Id),
				new SqlParameter("@nome", usuario.Nome),
				new SqlParameter("@email", usuario.Email),
				new SqlParameter("@data_nascimento", usuario.DataNascimento),
				new SqlParameter("@cep", usuario.Cep),
				new SqlParameter("@logradouro", usuario.Logradouro),
				new SqlParameter("@numero", usuario.Numero),
				new SqlParameter("@cidade", usuario.Cidade),
				new SqlParameter("@estado", usuario.Estado),
				new SqlParameter("@loginUsuario", usuario.LoginUsuario),
				new SqlParameter("@senha", usuario.Senha),
				new SqlParameter("@sexoId", usuario.SexoId),
                new SqlParameter("@IsAdmin", usuario.IsAdmin),
            };

            SqlParameter imagemParam = new SqlParameter("@imagem", SqlDbType.VarBinary, -1);

            if (usuario.ImagemEmByte != null && usuario.ImagemEmByte.Length > 0)
                imagemParam.Value = usuario.ImagemEmByte;
            else
                imagemParam.Value = DBNull.Value;

            parametros.Add(imagemParam);
            return parametros.ToArray();
        }

        /// <summary>
        /// Mapeia um DataRow para o UsuarioViewModel, incluindo IsAdmin.
        /// </summary>
        protected override UsuarioViewModel MontaModel(DataRow registro)
        {
            UsuarioViewModel usuario = new UsuarioViewModel();
            usuario.Id = Convert.ToInt32(registro["id"]);
            usuario.Nome = registro["nome"].ToString();
            usuario.Email = registro["email"].ToString();
            usuario.DataNascimento = Convert.ToDateTime(registro["data_nascimento"]);
            usuario.Cep = registro["cep"].ToString();
            usuario.Logradouro = registro["logradouro"].ToString();
            usuario.Numero = Convert.ToInt32(registro["numero"]);
            usuario.Cidade = registro["cidade"].ToString();
            usuario.Estado = registro["estado"].ToString();
            usuario.LoginUsuario = registro["loginUsuario"].ToString();
            usuario.Senha = registro["senha"].ToString();
            usuario.SexoId = Convert.ToInt32(registro["sexoid"]);
			usuario.IsAdmin = Convert.ToBoolean(registro["IsAdmin"]);

            if (registro["imagem"] != DBNull.Value)
                usuario.ImagemEmByte = registro["imagem"] as byte[];

            if (registro.Table.Columns.Contains("NomeSexo"))
                usuario.NomeSexo = registro["NomeSexo"].ToString();

            return usuario;
        }

        /// <summary>
        /// Sobrescreve Listagem para incluir IsAdmin no SELECT.
        /// </summary>
        public override List<UsuarioViewModel> Listagem()
        {
            var tabela = HelperDAO.ExecutaSelect($"select u.*, s.nome as NomeSexo from {Tabela} u left join sexos s on u.sexoId = s.id", null);
            List<UsuarioViewModel> lista = new List<UsuarioViewModel>();
            foreach (DataRow registro in tabela.Rows)
                lista.Add(MontaModel(registro));
            return lista;
        }

        /// <summary>
        /// Sobrescreve Consulta para incluir IsAdmin no SELECT.
        /// </summary>
        public override UsuarioViewModel Consulta(int id)
        {
            var p = new SqlParameter[] { new SqlParameter("id", id) };
            var tabela = HelperDAO.ExecutaSelect($"select u.*, s.nome as NomeSexo from {Tabela} u left join sexos s on u.sexoId = s.id where u.id = @id", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaModel(tabela.Rows[0]);
        }

        /// <summary>
        /// Sobrescreve Consulta(login, senha) para incluir IsAdmin no SELECT.
        /// </summary>
        public UsuarioViewModel Consulta(string login, string senha)
        {
            var p = new SqlParameter[]
            {
                new SqlParameter("@login", login),
                new SqlParameter("@senha", senha)
            };
            var tabela = HelperDAO.ExecutaSelect($"select u.*, s.nome as NomeSexo from {Tabela} u left join sexos s on u.sexoId = s.id where u.loginUsuario = @login and u.senha = @senha", p);
            if (tabela.Rows.Count == 0)
                return null;
            else
                return MontaModel(tabela.Rows[0]);
        }


        /// <summary>
        /// Executa uma consulta avançada de usuários com base em vários filtros,
        /// utilizando a stored procedure 'spConsultaAvancadaUsuarios'.
        /// </summary>
        /// <param name="nome">Filtro por nome de usuário.</param>
        /// <param name="estado">Filtro por estado.</param>
        /// <param name="sexoId">Filtro por ID de sexo.</param>
        /// <param name="dataInicial">Filtro de data de nascimento inicial.</param>
        /// <param name="dataFinal">Filtro de data de nascimento final.</param>
        /// <param name="login">Filtro por login de usuário.</param>
        /// <returns>Uma lista de UsuarioViewModel com os usuários que correspondem aos filtros.</returns>
        public List<UsuarioViewModel> ConsultaAvancadaUsuarios(string nome, string estado, int sexoId, DateTime dataInicial, DateTime dataFinal, string login)
        {
            SqlParameter[] p = {
                new SqlParameter("@nome", nome),
                new SqlParameter("@estado", estado),
                new SqlParameter("@sexoId", sexoId),
                new SqlParameter("@dataInicial", dataInicial),
                new SqlParameter("@dataFinal", dataFinal),
                new SqlParameter("@login", login)
            };
            var tabela = HelperDAO.ExecutaProcSelect("spConsultaAvancadaUsuarios", p);
            var lista = new List<UsuarioViewModel>();
            foreach (DataRow dr in tabela.Rows)
                lista.Add(MontaModel(dr));
            return lista;
        }
    }
}