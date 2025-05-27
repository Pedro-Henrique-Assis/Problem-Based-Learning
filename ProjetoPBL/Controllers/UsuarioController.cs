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

        /// <summary>
        /// Sobrescreve o método de redirecionamento para o UsuarioController.
        /// Se a operação for de Inserção ("I"), redireciona para a tela de Login.
        /// Caso contrário, mantém o comportamento padrão da PadraoController.
        /// </summary>
        /// <param name="operacao">A operação realizada ("I" para Insert, "A" para Update).</param>
        /// <returns>O IActionResult para o redirecionamento.</returns>
        protected override IActionResult GetSaveRedirectAction(string operacao)
        {
            if (operacao == "I")
            {
                // Redireciona para Login/Index especificamente na inserção de usuário
                return RedirectToAction("Index", "Login");
            }
            else
            {
                // Para outras operações (como Update), usa o comportamento padrão
                return base.GetSaveRedirectAction(operacao);
            }
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
                {
                    usuarioJaExiste = true;
                    break;
                }
                    
            }

            if (usuarioJaExiste)
                ModelState.AddModelError("LoginUsuario", "Usuário já cadastrado");

            // Processamento da Imagem
            if (ModelState.IsValid) // Processa imagem apenas se o restante do modelo for válido
            {
                if (usuario.RemoverImagemAtual)
                {
                    usuario.ImagemEmByte = null;
                    usuario.Imagem = null; // Garante que o IFormFile também seja nulo
                }
                else if (usuario.Imagem != null) // Nova imagem foi enviada
                {
                    usuario.ImagemEmByte = ConvertImageToByte(usuario.Imagem);
                }
                else if (operacao == "A") // É uma alteração e nenhuma nova imagem foi enviada E não é para remover
                {
                    // Mantém a imagem existente
                    UsuarioViewModel u = DAO.Consulta(usuario.Id);
                    if (u != null) // Verifica se o usuário consultado não é nulo
                    {
                        usuario.ImagemEmByte = u.ImagemEmByte;
                    }
                    else
                    {
                        // Se não encontrar o usuário (improvável aqui, mas por segurança)
                        usuario.ImagemEmByte = null;
                    }
                }
                else // É uma inserção e nenhuma imagem foi enviada
                {
                    usuario.ImagemEmByte = null;
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
