using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace ProjetoPBL.Controllers
{
    public class ChamadoController : Controller
    {
        // Instância do DAO responsável pelas operações com Chamados no banco de dados
        private readonly ChamadoDAO _chamadoDAO;

        /// <summary>
        /// Construtor do controller, inicializa a instância do ChamadoDAO
        /// </summary>
        public ChamadoController()
        {
            _chamadoDAO = new ChamadoDAO();
        }

        /// <summary>
        /// Verifica se o usuário está logado através da sessão
        /// </summary>
        /// <returns>True se usuário está logado, false caso contrário</returns>
        private bool IsUserLogged() =>
            HelperControllers.VerificaUserLogado(HttpContext.Session);

        /// <summary>
        /// Verifica se o usuário logado possui perfil de administrador
        /// </summary>
        /// <returns>True se usuário é administrador, false caso contrário</returns>
        private bool IsUserAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return !string.IsNullOrEmpty(isAdmin) && isAdmin.Equals("True", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtém o ID do usuário logado a partir da sessão
        /// </summary>
        /// <returns>ID do usuário</returns>
        /// <exception cref="Exception">Lançada caso o usuário não esteja logado</exception>
        private int GetUsuarioId()
        {
            var idStr = HttpContext.Session.GetString("IdUsuario");
            if (int.TryParse(idStr, out int id))
                return id;

            throw new Exception("Usuário não está logado.");
        }

        /// <summary>
        /// Exibe a lista de chamados disponíveis para o usuário logado, 
        /// considerando se ele é admin ou não
        /// </summary>
        /// <returns>View com a lista de chamados</returns>
        public IActionResult Index()
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                int usuarioId = GetUsuarioId();
                bool isAdmin = IsUserAdmin();

                // Busca chamados com base na permissão do usuário
                var chamados = _chamadoDAO.ConsultaChamadosPorPermissao(usuarioId, isAdmin);

                ViewBag.UsuarioId = usuarioId;
                ViewBag.IsAdmin = isAdmin;

                return View(chamados);
            }
            catch (Exception ex)
            {
                // Exibe página de erro em caso de exceção
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }

        /// <summary>
        /// Exibe todos os chamados - apenas para administradores
        /// </summary>
        /// <returns>View com todos os chamados</returns>
        public IActionResult Todos()
        {
            if (!IsUserLogged() || !IsUserAdmin())
                return RedirectToAction("Index", "Login");

            try
            {
                var chamados = _chamadoDAO.ListarTodos();
                return View("Index", chamados);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }

        /// <summary>
        /// Exibe o formulário para criação de novo chamado (GET)
        /// </summary>
        /// <returns>View com o formulário vazio</returns>
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            ViewBag.Operacao = "I"; // Indica operação de inserção
            return View("Form", new ChamadoViewModel());
        }

        /// <summary>
        /// Processa o envio do formulário para criação de chamado (POST)
        /// </summary>
        /// <param name="model">Modelo do chamado preenchido</param>
        /// <param name="retornarId">Flag para retornar o ID criado via JSON</param>
        /// <returns>Redirecionamento ou JSON conforme flag</returns>
        [HttpPost]
        public IActionResult Create(ChamadoViewModel model, bool retornarId = false)
        {
            if (!IsUserLogged())
                return Unauthorized(new { sucesso = false, mensagem = "Usuário não logado." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Define dados obrigatórios antes da inserção
                model.UsuarioId = GetUsuarioId();
                model.Status = "Aberto";
                model.DataAbertura = DateTime.Now;

                int novoId = _chamadoDAO.InsertRetornandoId(model);

                if (retornarId)
                    return Json(new { sucesso = true, chamadoId = novoId });

                TempData["Sucesso"] = "Chamado criado com sucesso.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (retornarId)
                    return Json(new { sucesso = false, mensagem = ex.Message });

                ViewBag.Erro = ex.Message;
                return View("Form", model);
            }
        }

        /// <summary>
        /// Exibe o formulário para edição de chamado (GET)
        /// </summary>
        /// <param name="id">ID do chamado a ser editado</param>
        /// <returns>View com o formulário preenchido</returns>
        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                var chamado = _chamadoDAO.Consulta(id);

                // Verifica se o chamado pertence ao usuário logado
                if (chamado == null || chamado.UsuarioId != GetUsuarioId())
                    return RedirectToAction("Index");

                ViewBag.Operacao = "E"; // Indica operação de edição
                return View("Form", chamado);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }

        /// <summary>
        /// Processa o envio do formulário para edição de chamado (POST)
        /// </summary>
        /// <param name="model">Modelo do chamado com dados atualizados</param>
        /// <returns>Redirecionamento ou reexibição do formulário em caso de erro</returns>
        [HttpPost]
        public IActionResult Editar(ChamadoViewModel model)
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Operacao = "E";
                    return View("Form", model);
                }

                var chamadoDb = _chamadoDAO.Consulta(model.Id);

                // Verifica se o chamado pertence ao usuário logado
                if (chamadoDb == null || chamadoDb.UsuarioId != GetUsuarioId())
                    return RedirectToAction("Index");

                // Atualiza campos editáveis
                chamadoDb.Titulo = model.Titulo;
                chamadoDb.Descricao = model.Descricao;

                _chamadoDAO.Update(chamadoDb);

                TempData["Sucesso"] = "Chamado atualizado com sucesso.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Erro = ex.Message;
                ViewBag.Operacao = "E";
                return View("Form", model);
            }
        }

        /// <summary>
        /// Exibe o formulário para exclusão do chamado (GET)
        /// </summary>
        /// <param name="id">ID do chamado a ser excluído</param>
        /// <returns>View com dados para confirmação da exclusão</returns>
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                var chamado = _chamadoDAO.Consulta(id);

                // Verifica se o chamado pertence ao usuário logado
                if (chamado == null || chamado.UsuarioId != GetUsuarioId())
                    return RedirectToAction("Index");

                ViewBag.Operacao = "D"; // Indica operação de exclusão
                return View("Form", chamado);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }

        /// <summary>
        /// Processa a exclusão confirmada do chamado (POST)
        /// </summary>
        /// <param name="id">ID do chamado a ser excluído</param>
        /// <returns>Redirecionamento após exclusão ou erro</returns>
        [HttpPost]
        public IActionResult ExcluirConfirmado(int id)
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                var chamado = _chamadoDAO.Consulta(id);

                // Verifica se o chamado pertence ao usuário logado
                if (chamado == null || chamado.UsuarioId != GetUsuarioId())
                    return RedirectToAction("Index");

                int usuarioIdLogado = GetUsuarioId();
                bool isAdmin = IsUserAdmin();

                // Somente admin ou dono do chamado podem excluir
                if (!isAdmin && chamado.UsuarioId != usuarioIdLogado)
                    return RedirectToAction("Index");

                _chamadoDAO.Delete(id);

                TempData["Sucesso"] = "Chamado excluído com sucesso.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Exibe formulário para resposta a chamado - somente administradores
        /// </summary>
        /// <param name="id">ID do chamado para responder</param>
        /// <returns>View com formulário para resposta</returns>
        [HttpGet]
        public IActionResult Responder(int id)
        {
            if (!IsUserLogged() || !IsUserAdmin())
                return RedirectToAction("Index", "Login");

            try
            {
                var chamado = _chamadoDAO.Consulta(id);
                if (chamado == null)
                    return RedirectToAction("Todos");

                ViewBag.Operacao = "R"; // Indica operação de resposta
                return View("Form", chamado);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }

        /// <summary>
        /// Processa o envio da resposta e alteração de status do chamado (POST)
        /// </summary>
        /// <param name="id">ID do chamado</param>
        /// <param name="resposta">Resposta fornecida pelo admin</param>
        /// <param name="status">Novo status do chamado</param>
        /// <returns>Redirecionamento após envio</returns>
        [HttpPost]
        public IActionResult Responder(int id, string resposta, string status)
        {
            if (!IsUserLogged() || !IsUserAdmin())
                return RedirectToAction("Index", "Login");

            try
            {
                _chamadoDAO.Responder(id, resposta, status);
                TempData["Sucesso"] = "Resposta enviada com sucesso.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
