using ProjetoPBL.Models;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace ProjetoPBL.DAO
{
    public class UsuarioDAO : PadraoDAO<UsuarioViewModel>
    {
        protected override SqlParameter[] CriaParametros(UsuarioViewModel usuario)
        {
			List<SqlParameter> parametros = new List<SqlParameter>
			{
				new SqlParameter("id", usuario.Id),
				new SqlParameter("nome", usuario.Nome),
				new SqlParameter("email", usuario.Email),
				new SqlParameter("data_nascimento", usuario.DataNascimento),
				new SqlParameter("cep", usuario.Cep),
				new SqlParameter("logradouro", usuario.Logradouro),
				new SqlParameter("numero", usuario.Numero),
				new SqlParameter("cidade", usuario.Cidade),
				new SqlParameter("estado", usuario.Estado),
				new SqlParameter("loginUsuario", usuario.LoginUsuario),
				new SqlParameter("senha", usuario.Senha),
				new SqlParameter("sexoId", usuario.SexoId),
			};

            if (usuario.ImagemEmByte != null)
                parametros.Add(new SqlParameter("imagem", usuario.ImagemEmByte));

            return parametros.ToArray();
        }

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

            if (registro["imagem"] != DBNull.Value)
                usuario.ImagemEmByte = registro["imagem"] as byte[];

            if (registro.Table.Columns.Contains("NomeSexo"))
                usuario.NomeSexo = registro["NomeSexo"].ToString();

            return usuario;
        }

        protected override void SetTabela()
        {
            Tabela = "usuarios";
            NomeSpListagem = "spListagem";
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
	' where id = ' + cast(@id as varchar(max)) 
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
	@imagem varbinary(max)
)
as
begin
	insert into usuarios
		(id, nome, email, data_nascimento, cep, logradouro, numero, cidade, estado, loginUsuario, senha, sexoId, imagem)
	values
		(@id, @nome, @email, @data_nascimento, @cep, @logradouro, @numero, @cidade, @estado, @loginUsuario, @senha, @sexoId, @imagem)
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
	@imagem varbinary(max)
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
		   imagem = @imagem
	where id = @id
end
go
*/
