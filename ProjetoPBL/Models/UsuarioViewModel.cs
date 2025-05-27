using Microsoft.AspNetCore.Http;
using System;

namespace ProjetoPBL.Models
{
    public class UsuarioViewModel : PadraoViewModel
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Cep { get; set; }
        public string Logradouro { get; set; }
        public int Numero { get; set; } //Número do endereço do usuário
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string LoginUsuario { get; set; }
        public string Senha { get; set; }
        public int SexoId { get; set; }

        //Campo trazido pelo left join, pela chave SexoId
        public string NomeSexo { get; set; }

        /// <summary> 
        /// Imagem recebida do form pelo controller 
        /// </summary> 
        public IFormFile Imagem { get; set; }

        /// <summary> 
        /// Imagem em bytes pronta para ser salva 
        /// </summary> 
        public byte[]? ImagemEmByte { get; set; }

        /// <summary> 
        /// Imagem usada para ser enviada ao form no formato para ser exibida 
        /// </summary> 
        public string ImagemEmBase64
        {
            get
            {
                if (ImagemEmByte != null)
                    return Convert.ToBase64String(ImagemEmByte);
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Indica se o usuário solicitou a remoção da imagem de perfil atual.
        /// </summary>
        public bool RemoverImagemAtual { get; set; } = false;

        /// <summary>
        /// Indica se o usuário é um administrador.
        /// </summary>
        public bool IsAdmin { get; set; } = false;
    }
}

/*
create database projeto_pbl
go

use projeto_pbl
go

create table usuarios (
	id int not null primary key,
	nome varchar(max) not null,
	email varchar(max) not null,
	data_nascimento datetime not null,
	cep varchar(max) not null,
	logradouro varchar(max) not null,
	numero int not null,
	cidade varchar(max) not null,
	estado varchar(max) not null,
	loginUsuario varchar(max) not null,
	senha varchar(max) not null,
	sexoId int not null
)
*/