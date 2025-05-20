using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ProjetoPBL.Controllers
{
    public class UsuarioController : PadraoController<UsuarioViewModel>
    {
        public UsuarioController() 
        {
            DAO = new UsuarioDAO();
            GeraProximoId = true;
            ExigeAutenticacao = false;
        }

        protected override void ValidaDados(UsuarioViewModel usuario, string operacao)
        {
            bool usuarioJaExiste = false;
            var usuarioDAO = new UsuarioDAO();
            var lista = usuarioDAO.Listagem();

            //Mantém a validação de Id presente na PadraoController
            base.ValidaDados(usuario, operacao);

            if (string.IsNullOrEmpty(usuario.Nome))
                ModelState.AddModelError("Nome", "Preencha o nome.");
            if (!Validador.ValidaEmail(usuario.Email))
                ModelState.AddModelError("Email", "O formato de email está incorreto. Modelo: prefixo@sufixo.com(.br)");
            if (usuario.DataNascimento > DateTime.Now)
                ModelState.AddModelError("DataNascimento", "Data inválida!");
            if (!Validador.ValidaCep(usuario.Cep))
                ModelState.AddModelError("Cep", "O formato do cep está incorreto. Modelo: xxxxx-xxx");
            if (string.IsNullOrEmpty(usuario.Logradouro))
                ModelState.AddModelError("Logradouro", "Preencha o logradouro.");
            if (usuario.Numero <= 0)
                ModelState.AddModelError("Numero", "Preencha um número válido.");
            if (string.IsNullOrEmpty(usuario.Cidade))
                ModelState.AddModelError("Cidade", "Preencha a cidade.");
            if (string.IsNullOrEmpty(usuario.Estado))
                ModelState.AddModelError("Estado", "Preencha o estado.");
            if (usuario.SexoId <= 0)
                ModelState.AddModelError("SexoId", "Escolha um sexo válido.");
            if (usuario.Imagem != null && usuario.Imagem.Length / 1024 / 1024 >= 2.048)
                ModelState.AddModelError("Imagem", "Imagem limitada a 2 mb.");

            foreach (var usuarioCadastrado in lista)
            {
                if (usuarioCadastrado.LoginUsuario == usuario.LoginUsuario)
                    usuarioJaExiste = true;
            }

            if (usuarioJaExiste)
                ModelState.AddModelError("LoginUsuario", "Usuário já cadastrado");

            if (ModelState.IsValid)
            {
                //Na alteração, se não for informada a imagem, será mantida a que já estava salva.
                if (operacao == "A" && usuario.Imagem == null)
                {
                    UsuarioViewModel u = DAO.Consulta(usuario.Id);
                    usuario.ImagemEmByte = u.ImagemEmByte;
                }
                else
                {
                    usuario.ImagemEmByte = ConvertImageToByte(usuario.Imagem);
                }
            }
        }

        private void PreparaListaSexosParaCombo()
        {
            var sexoDAO = new SexoDAO();
            var sexos = sexoDAO.Listagem();
            List<SelectListItem> listaSexos = new List<SelectListItem>();
            listaSexos.Add(new SelectListItem("Selecione seu gênero...", "0"));
            foreach (var sexo in sexos)
            {
                SelectListItem item = new SelectListItem(sexo.Nome, sexo.Id.ToString());
                listaSexos.Add(item);
            }
            ViewBag.Sexos = listaSexos;
        }

        protected override void PreencheDadosParaView(string Operacao, UsuarioViewModel model)
        {
            base.PreencheDadosParaView(Operacao, model);
            if (Operacao == "I")
                model.DataNascimento = DateTime.Now;

            PreparaListaSexosParaCombo();
        }

        public byte[] ConvertImageToByte(IFormFile file)
        {
            if (file != null)
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    return ms.ToArray();
                }
            else
                return null;
        }
    }
}
