using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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

        [HttpGet]
        public IActionResult TrocaSenha()
        {
            // A troca de senha não exige autenticação para ser acessada
            ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);
            return View(new TrocaSenhaViewModel());
        }

        // POST: Usuario/SalvarNovaSenha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalvarNovaSenha(TrocaSenhaViewModel model)
        {
            ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);

            if (!ModelState.IsValid)
            {
                return View("TrocaSenha", model);
            }

            var usuarioDAO = (UsuarioDAO)DAO; // Cast para ter acesso aos métodos específicos se necessário
            var listaUsuarios = usuarioDAO.Listagem();
            UsuarioViewModel usuarioParaAtualizar = listaUsuarios.FirstOrDefault(u => u.LoginUsuario == model.LoginUsuario);

            if (usuarioParaAtualizar == null)
            {
                ModelState.AddModelError("LoginUsuario", "Usuário não encontrado.");
                return View("TrocaSenha", model);
            }

            if (usuarioParaAtualizar.Senha == model.NovaSenha)
            {
                ModelState.AddModelError("NovaSenha", "A nova senha não pode ser igual à senha antiga.");
                return View("TrocaSenha", model);
            }

            // Atualiza apenas a senha do usuário
            usuarioParaAtualizar.Senha = model.NovaSenha;

            try
            {
                // Como o método Update do PadraoDAO espera o modelo completo,
                // e a stored procedure spUpdate_usuarios atualiza todos os campos,
                // precisamos garantir que os outros campos não sejam alterados indevidamente.
                // O DAO.Update já recebe o usuarioParaAtualizar que contém todos os dados
                // originais, exceto a senha que acabamos de modificar.
                DAO.Update(usuarioParaAtualizar);

                TempData["MensagemSucesso"] = "Senha alterada com sucesso! Você já pode fazer login com a nova senha.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                ViewBag.Erro = "Ocorreu um erro ao tentar atualizar a senha.";
                return View("TrocaSenha", model);
            }
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
            else if (operacao == "A")
            {
                TempData["MensagemSucessoAdmin"] = "Usuário atualizado com sucesso!";
                return RedirectToAction("ConsultaAvancada", "Admin");
            }
            else
            {
                return base.GetSaveRedirectAction(operacao);
            }
        }

        protected override void ValidaDados(UsuarioViewModel usuario, string operacao)
        {
            bool usuarioJaExiste = ((UsuarioDAO)DAO).Listagem().Any(u => u.LoginUsuario == usuario.LoginUsuario && u.Id != usuario.Id);

            //Mantém a validação de Id presente na PadraoController
            base.ValidaDados(usuario, operacao);

            if (string.IsNullOrEmpty(usuario.Nome))
                ModelState.AddModelError("Nome", "Preencha o nome.");
            else
            {
                //Expressão regular para validação extra do nome
                var nomeRegex = new Regex(@"^[a-zA-ZÀ-ú\s']{2,200}$");
                if (!nomeRegex.IsMatch(usuario.Nome))
                {
                    ModelState.AddModelError("Nome", "O Nome deve conter apenas letras, espaços e apóstrofos (')");
                }
                else if (usuario.Nome.Trim().Length < 2)
                {
                    ModelState.AddModelError("Nome", "O Nome deve ter pelo menos 2 caracteres.");
                }
            }
            if (!Validador.ValidaEmail(usuario.Email))
                ModelState.AddModelError("Email", "O formato de email está incorreto. Modelo: prefixo@sufixo.com(.br)");

            if (usuario.DataNascimento > DateTime.Now)
                ModelState.AddModelError("DataNascimento", "Data inválida!");
            else if (usuario.DataNascimento == DateTime.MinValue) // Verifica se a data foi deixada vazia
                ModelState.AddModelError("DataNascimento", "A data de nascimento é obrigatória.");

            if (!Validador.ValidaCep(usuario.Cep))
            ModelState.AddModelError("Cep", "O formato do cep está incorreto. Modelo: xxxxx-xxx ou xxxxxxxx");
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

            if (usuarioJaExiste)
                ModelState.AddModelError("LoginUsuario", "Usuário já cadastrado");

            // Na inserção, a senha é obrigatória
            if (operacao == "I" && string.IsNullOrEmpty(usuario.Senha))
                ModelState.AddModelError("Senha", "A senha é obrigatória.");

            if (ModelState.IsValid)
            {
                // Se for uma alteração, busca os dados atuais para manter
                // a imagem e a senha, caso não sejam alteradas.
                if (operacao == "A")
                {
                    // Evita consulta dupla ao banco se já tiver os dados.
                    UsuarioViewModel usuarioAtual = DAO.Consulta(usuario.Id);
                    if (usuarioAtual != null)
                    {
                        // Lógica da Senha: Se o campo veio vazio, mantém a senha antiga.
                        if (string.IsNullOrEmpty(usuario.Senha))
                        {
                            usuario.Senha = usuarioAtual.Senha;
                        }

                        string loggedInUserIdStr = HttpContext.Session.GetString("IdUsuario");
                        string loggedInUserIsAdminSessionStr = HttpContext.Session.GetString("IsAdmin");
                        bool editorIsAdmin = loggedInUserIsAdminSessionStr == "True";
                        int.TryParse(loggedInUserIdStr, out int loggedInUserId);
                        bool isAdminFieldPresentInForm = Request.Form.ContainsKey(nameof(UsuarioViewModel.IsAdmin));

                        if (editorIsAdmin)
                        {
                            if (usuario.Id == loggedInUserId)
                            {
                                usuario.IsAdmin = usuarioAtual.IsAdmin;

                                if (usuarioAtual.IsAdmin && !usuario.IsAdmin && Request.Form.ContainsKey(nameof(UsuarioViewModel.IsAdmin)))
                                {
                                    usuario.IsAdmin = true;
                                    ModelState.AddModelError("IsAdmin", "Você não pode remover seu próprio status de administrador."); // Opcional
                                }
                            }
                            else
                            {
                                // Admin está editando outro usuário.
                                if (usuarioAtual.IsAdmin && !usuario.IsAdmin)
                                {
                                    var todosAdmins = ((UsuarioDAO)DAO).Listagem().Where(u => u.IsAdmin).ToList();
                                    // Se o usuário atual é o único admin na lista (ou o único que está sendo desmarcado)
                                    if (todosAdmins.Count(a => a.Id != usuario.Id || a.IsAdmin) <= 0 && todosAdmins.Any(a => a.Id == usuario.Id))
                                    {
                                        ModelState.AddModelError("IsAdmin", "Não é possível remover o status de administrador do último administrador do sistema.");
                                        usuario.IsAdmin = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Se quem edita não é admin, o status IsAdmin não pode ser alterado.
                            usuario.IsAdmin = usuarioAtual.IsAdmin;
                        }

                        // Lógica da Imagem: Se uma nova imagem não foi enviada E não foi solicitado para remover,
                        // mantém a imagem antiga.
                        if (usuario.Imagem == null && !usuario.RemoverImagemAtual)
                        {
                            usuario.ImagemEmByte = usuarioAtual.ImagemEmByte;
                        }
                    }
                }

                // Esta parte da sua lógica de imagem original continua válida
                if (usuario.RemoverImagemAtual)
                {
                    usuario.ImagemEmByte = null;
                }
                else if (usuario.Imagem != null) // Nova imagem foi enviada
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

        public override IActionResult Edit(int id)
        {
            ViewBag.Operacao = "A";
            var model = DAO.Consulta(id);

            if (model == null)
                return RedirectToAction(NomeViewIndex);

            string loggedInUserIdStr = HttpContext.Session.GetString("IdUsuario");
            int.TryParse(loggedInUserIdStr, out int loggedInUserId);
            string loggedInUserIsAdminSessionStr = HttpContext.Session.GetString("IsAdmin");
            bool editorLogadoEAdmin = loggedInUserIsAdminSessionStr == "True";

            ViewBag.UsuarioLogadoEAdmin = editorLogadoEAdmin;
            ViewBag.EditandoProprioPerfil = (model.Id == loggedInUserId);

            // Captura o parâmetro 'source' da query string
            if (Request.Query.ContainsKey("source"))
            {
                ViewBag.Source = Request.Query["source"].ToString();
            }

            PreencheDadosParaView("A", model);
            return View(NomeViewForm, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override IActionResult Save(UsuarioViewModel model, string Operacao)
        {
            string source = Request.Form["Source"];

            try
            {
                if (Operacao == "I" && GeraProximoId && model.Id == 0) // Segurança extra caso o ID não tenha sido gerado
                {
                    model.Id = DAO.ProximoId();
                }

                ValidaDados(model, Operacao);
                if (!ModelState.IsValid)
                {
                    ViewBag.Operacao = Operacao;
                    ViewBag.Source = source;
                    PreencheDadosParaView(Operacao, model);
                    return View(NomeViewForm, model);
                }
                else
                {
                    if (Operacao == "I")
                        DAO.Insert(model);
                    else
                        DAO.Update(model);

                    return GetSaveRedirectAction(Operacao, source);
                }
            }
            catch (Exception erro)
            {
                ViewBag.Operacao = Operacao;
                ViewBag.Source = source;
                PreencheDadosParaView(Operacao, model);
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao salvar as alterações: " + erro.Message);
                return View(NomeViewForm, model);
            }
        }

        protected IActionResult GetSaveRedirectAction(string operacao, string source = null)
        {
            if (operacao == "I")
            {
                return RedirectToAction("Index", "Login");
            }
            else if (operacao == "A")
            {
                if ("profile".Equals(source, StringComparison.OrdinalIgnoreCase))
                {
                    TempData["MensagemSucesso"] = "Perfil atualizado com sucesso!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["MensagemSucessoAdmin"] = "Usuário atualizado com sucesso!";
                    return RedirectToAction("ConsultaAvancada", "Admin");
                }
            }
            return RedirectToAction(NomeViewIndex);
        }

    }
}
