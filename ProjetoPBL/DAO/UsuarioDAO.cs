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

        public List<UsuarioViewModel> ConsultaAvancadaUsuarios(string nome, string estado, int sexoId, DateTime dataInicial, DateTime dataFinal)
        {
            SqlParameter[] p = {
                new SqlParameter("@nome", nome),
                new SqlParameter("@estado", estado),
                new SqlParameter("@sexoId", sexoId),
                new SqlParameter("@dataInicial", dataInicial),
                new SqlParameter("@dataFinal", dataFinal),
            };
            var tabela = HelperDAO.ExecutaProcSelect("spConsultaAvancadaUsuarios", p);
            var lista = new List<UsuarioViewModel>();
            foreach (DataRow dr in tabela.Rows)
                lista.Add(MontaModel(dr));
            return lista;
        }
    }
}

/*
create procedure spDelete
(
	@id int,
	@tabela varchar(max)
)
as
begin
	declare @sql varchar(max)
	set @sql = 'delete ' + @tabela +
		' where id = ' + cast(@id as varchar(max))
	exec(@sql)
end
go


create procedure spConsulta 
( 
	@id int , 
	@tabela varchar(max) 
) 
as 
begin 
	declare @sql varchar(max); 
	set @sql = 'select * from ' + @tabela + 
	'  where id = ' + cast(@id as varchar(max)) 
	exec(@sql)
end
go

create procedure spListagem 
( 
	@tabela varchar(max)
)
as 
begin 
	exec('select * from ' + @tabela) 
end 
go


create procedure spProximoId 
(@tabela varchar(max)) 
as 
begin
	exec('select isnull(max(id) +1, 1) as MAIOR from '
		+ @tabela)
end 
go


create procedure spInsert_usuarios
(
	@id int,
	@nome varchar(max),
	@email varchar(max),
	@data_nascimento datetime,
	@cep varchar(max),
	@logradouro varchar(max),
	@numero int,
	@cidade varchar(max),
	@estado varchar(max),
	@loginUsuario varchar(max),
	@senha varchar(max),
	@sexoId int,
	@imagem varbinary(max),
    @IsAdmin bit
)
as
begin
	insert into usuarios
		(id, nome, email, data_nascimento, cep, logradouro, numero, cidade, estado, loginUsuario, senha, sexoId, imagem, IsAdmin)
	values
		(@id, @nome, @email, @data_nascimento, @cep, @logradouro, @numero, @cidade, @estado, @loginUsuario, @senha, @sexoId, @imagem, @IsAdmin)
end
go


create procedure spUpdate_usuarios
(
	@id int,
	@nome varchar(max),
	@email varchar(max),
	@data_nascimento datetime,
	@cep varchar(max),
	@logradouro varchar(max),
	@numero int,
	@cidade varchar(max),
	@estado varchar(max),
	@loginUsuario varchar(max),
	@senha varchar(max),
	@sexoId int,
	@imagem varbinary(max),
    @IsAdmin bit
)
as
begin
	update usuarios set
		   nome = @nome,
		   email = @email,
		   data_nascimento = @data_nascimento,
		   cep = @cep,
		   logradouro = @logradouro,
		   numero = @numero,
		   cidade = @cidade,
		   estado = @estado,
		   loginUsuario = @loginUsuario,
		   senha = @senha,
		   sexoId = @sexoId,
		   imagem = @imagem,
           IsAdmin = @IsAdmin
	where id = @id
end
go

create procedure [dbo].[spConsultaAvancadaUsuarios] 
( 
	@nome varchar(max), 
	@estado varchar(max),
	@sexoId int,
	@dataInicial datetime, 
	@dataFinal datetime) 
as 
begin 
	declare @categIni int 
	declare @categFim int 
	set @categIni = case @sexoId when 0 then 0 else @sexoId end
	set @categFim = case @sexoId when 0 then 999999 else @sexoId end 
	select usuarios.*, sexos.nome as 'NomeSexo' 
	from usuarios 
	inner join sexos on usuarios.sexoId = sexos.Id 
	where usuarios.nome like '%' + @nome + '%' and
	usuarios.estado like '%' + @estado + '%' and
	usuarios.data_nascimento between @dataInicial and @dataFinal and 
	usuarios.sexoId between @categIni and @categFim; 
end
go
*/
