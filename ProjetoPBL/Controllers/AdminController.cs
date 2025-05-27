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
            return HttpContext.Session.GetString("Logado") == "True" &&
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
                    return RedirectToAction("Index", "Login");

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
            // Proteção: Somente admins podem acessar
            if (!IsUserAdmin())
                return RedirectToAction("Index", "Login");

            try
            {
                var usuarioLogadoId = Convert.ToInt32(HttpContext.Session.GetString("IdUsuario"));
                if (id == usuarioLogadoId)
                {
                    TempData["ErroExcluir"] = "Você não pode excluir sua própria conta.";
                }
                else
                {
                    // Verificar se é o último admin antes de excluir
                    var usuarioParaExcluir = _usuarioDAO.Consulta(id);
                    var todosAdmins = _usuarioDAO.Listagem().Where(u => u.IsAdmin).ToList();

                    if (usuarioParaExcluir.IsAdmin && todosAdmins.Count <= 1)
                    {
                        TempData["ErroExcluir"] = "Não é possível excluir o último administrador do sistema.";
                    }
                    else
                    {
                        _usuarioDAO.Delete(id);
                        TempData["SucessoExcluir"] = "Usuário excluído com sucesso.";
                    }
                }
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }

            return RedirectToAction("ConsultaAvancada");
        }




        public IActionResult ObtemDadosConsultaAvancada(string nome, string estado, int sexoId, DateTime dataInicial, DateTime dataFinal)
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

                var lista = dao.ConsultaAvancadaUsuarios(nome, estado, sexoId, dataInicial, dataFinal);
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
    }
}
