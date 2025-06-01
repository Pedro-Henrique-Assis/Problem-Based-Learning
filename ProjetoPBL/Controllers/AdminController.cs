using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace ProjetoPBL.Controllers
{
    public class AdminController : Controller
    {
        private readonly UsuarioDAO _usuarioDAO;
        private readonly SexoDAO _sexoDAO;

        public AdminController()
        {
            _usuarioDAO = new UsuarioDAO();
            _sexoDAO = new SexoDAO();
        }

        /// <summary>
        /// Verifica se o usuário logado é um administrador.
        /// </summary>
        private bool IsUserAdmin()
        {
            return HttpContext.Session.GetString("Logado") == "true" &&
                   HttpContext.Session.GetString("IsAdmin") == "True";
        }

        /// <summary>
        /// Prepara a lista de sexos para ser usada no dropdown de filtro.
        /// </summary>
        private void PreparaListaSexosParaFiltro()
        {
            var sexos = _sexoDAO.Listagem();
            var listaSexos = new List<SelectListItem>();

            // Adiciona uma opção padrão para "Todos" no início da lista
            listaSexos.Add(new SelectListItem("Todos", "0"));

            foreach (var sexo in sexos)
            {
                SelectListItem item = new SelectListItem(sexo.Nome, sexo.Id.ToString());
                listaSexos.Add(item);
            }
            // Armazena a lista no ViewBag para ser acessada pela View
            ViewBag.Sexos = listaSexos;
        }

        /// <summary>
        /// Action para exibir a lista de usuários (Apenas Admins).
        /// </summary>
        public IActionResult ConsultaAvancada()
        {
            try
            {
                // Proteção: Somente admins podem acessar
                if (!IsUserAdmin())
                    return RedirectToAction("Index", "Home");

                ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);
                PreparaListaSexosParaFiltro();

                // Passa mensagens de TempData para a View, se existirem
                ViewBag.SucessoExcluir = TempData["SucessoExcluir"];
                ViewBag.ErroExcluir = TempData["ErroExcluir"];

                return View("ConsultaAvancada");
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.Message));
            }
            
        }

        /// <summary>
        /// Action para excluir um usuário (Apenas Admins).
        /// </summary>
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!IsUserAdmin())
            {
                // Para requisições AJAX, retornar um status de não autorizado
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { sucesso = false, mensagem = "Acesso não autorizado." });
                }
                return RedirectToAction("Index", "Home");
            }

            string mensagemSucesso = null;
            string mensagemErro = null;

            try
            {
                var usuarioLogadoId = Convert.ToInt32(HttpContext.Session.GetString("IdUsuario"));
                if (id == usuarioLogadoId)
                {
                    mensagemErro = "Você não pode excluir sua própria conta.";
                }
                else
                {
                    var usuarioParaExcluir = _usuarioDAO.Consulta(id);
                    if (usuarioParaExcluir == null) // Adiciona verificação se usuário existe
                    {
                        mensagemErro = "Usuário não encontrado para exclusão.";
                    }
                    else
                    {
                        var todosAdmins = _usuarioDAO.Listagem().Where(u => u.IsAdmin).ToList();
                        if (usuarioParaExcluir.IsAdmin && todosAdmins.Count <= 1)
                        {
                            mensagemErro = "Não é possível excluir o último administrador do sistema.";
                        }
                        else
                        {
                            _usuarioDAO.Delete(id);
                            mensagemSucesso = "Usuário excluído com sucesso.";
                        }
                    }
                }
            }
            catch (Exception erro)
            {
                mensagemErro = "Ocorreu um erro ao tentar excluir o usuário.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (!string.IsNullOrEmpty(mensagemErro))
                {
                    return Json(new { sucesso = false, mensagem = mensagemErro });
                }
                return Json(new { sucesso = true, mensagem = mensagemSucesso });
            }

            if (!string.IsNullOrEmpty(mensagemErro)) TempData["ErroExcluir"] = mensagemErro;
            if (!string.IsNullOrEmpty(mensagemSucesso)) TempData["SucessoExcluir"] = mensagemSucesso;

            return RedirectToAction("ConsultaAvancada");
        }



        /// <summary>
        /// Action para obter dados de usuários com base em filtros avançados.
        /// Este método é chamado para popular uma grade de usuários (PartialView "pvGridUsuarios")
        /// com base nos critérios de pesquisa fornecidos.
        /// </summary>
        /// <param name="nome">Nome do usuário para filtro (parcial ou completo).</param>
        /// <param name="estado">Estado do usuário para filtro.</param>
        /// <param name="sexoId">ID do sexo do usuário para filtro (0 para todos).</param>
        /// <param name="dataInicial">Data inicial para filtro de data de nascimento.</param>
        /// <param name="dataFinal">Data final para filtro de data de nascimento.</param>
        /// <param name="login">Login do usuário para filtro (parcial ou completo).</param>
        /// <returns>
        /// Retorna uma PartialView ("pvGridUsuarios") com a lista de usuários filtrada
        /// em caso de sucesso.
        /// Retorna um Json com uma mensagem de erro em caso de exceção.
        /// </returns>
        public IActionResult ObtemDadosConsultaAvancada(string nome, string estado, int sexoId, DateTime dataInicial, DateTime dataFinal, string login)
        {
            try
            {
                UsuarioDAO dao = new UsuarioDAO();
                if (string.IsNullOrEmpty(nome))
                    nome = "";

                if (string.IsNullOrEmpty(estado))
                    estado = "";

                if (dataInicial.Date == Convert.ToDateTime("01/01/0001"))
                    dataInicial = SqlDateTime.MinValue.Value;

                if (dataFinal.Date == Convert.ToDateTime("01/01/0001"))
                    dataFinal = SqlDateTime.MaxValue.Value;

                if (string.IsNullOrEmpty(login))
                    login = "";

                var lista = dao.ConsultaAvancadaUsuarios(nome, estado, sexoId, dataInicial, dataFinal, login);
                return PartialView("pvGridUsuarios", lista);
            }
            catch (Exception erro)
            {
                return Json(new { erro = true, msg = erro.Message });
            }
        }

        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// Action para obter e exibir a lista de chamados (tickets) para administradores.
        /// Este método verifica se o usuário logado é um administrador. Se for,
        /// busca todos os chamados ou os chamados pertinentes à visão de um administrador
        /// e os exibe em uma PartialView.
        /// </summary>
        /// <returns>
        /// Retorna uma PartialView ("~/Views/Chamado/pvGridChamados.cshtml") com a lista de chamados
        /// se o usuário for um administrador.
        /// Retorna um resultado UnauthorizedResult (HTTP 401) se o usuário não for administrador,
        /// indicando que o acesso não é permitido.
        /// </returns>
        public IActionResult ObtemChamados()
        {
            if (!IsUserAdmin())
                return Unauthorized();

            var usuarioId = Convert.ToInt32(HttpContext.Session.GetString("IdUsuario"));
            var chamadoDao = new ChamadoDAO();
            var chamados = chamadoDao.ConsultaChamadosPorPermissao(usuarioId, true);


            return PartialView("~/Views/Chamado/pvGridChamados.cshtml", chamados);
        }
    }
}
