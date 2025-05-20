using ProjetoPBL.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ProjetoPBL.DAO
{
    public class SexoDAO : PadraoDAO<SexoViewModel>
    {
        protected override SqlParameter[] CriaParametros(SexoViewModel model)
        {
            SqlParameter[] parametros =
            {
                 new SqlParameter("id", model.Id),
                 new SqlParameter("nome", model.Nome)
            };
            return parametros;
        }

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

/*
create table sexos (
	id int not null primary key,
    nome varchar(max) not null
)
go

insert into sexos values
(1, 'Masculino'),
(2, 'Feminino'),
(3, 'Outro'),
(4, 'Prefiro não informar')
*/