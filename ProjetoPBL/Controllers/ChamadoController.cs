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
        private readonly ChamadoDAO _chamadoDAO;

        public ChamadoController()
        {
            _chamadoDAO = new ChamadoDAO();
        }

        private bool IsUserLogged() =>
            HelperControllers.VerificaUserLogado(HttpContext.Session);

        private bool IsUserAdmin()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return !string.IsNullOrEmpty(isAdmin) && isAdmin.Equals("True", StringComparison.OrdinalIgnoreCase);
        }

        private int GetUsuarioId()
        {
            var idStr = HttpContext.Session.GetString("IdUsuario");
            if (int.TryParse(idStr, out int id))
                return id;

            throw new Exception("Usuário não está logado.");
        }


        public IActionResult Index()
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                int usuarioId = GetUsuarioId();
                bool isAdmin = IsUserAdmin();

                var chamados = _chamadoDAO.ConsultaChamadosPorPermissao(usuarioId, isAdmin);

                ViewBag.UsuarioId = usuarioId;
                ViewBag.IsAdmin = isAdmin;

                return View(chamados);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }



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


        [HttpGet]
        public IActionResult Create()
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            ViewBag.Operacao = "I"; 
            return View("Form", new ChamadoViewModel());
        }



        [HttpPost]
        public IActionResult Create(ChamadoViewModel model, bool retornarId = false)
        {
            if (!IsUserLogged())
                return Unauthorized(new { sucesso = false, mensagem = "Usuário não logado." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

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




        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                var chamado = _chamadoDAO.Consulta(id);
                if (chamado == null || chamado.UsuarioId != GetUsuarioId())
                    return RedirectToAction("Index");

                ViewBag.Operacao = "E";

                return View("Form", chamado);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }


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
                if (chamadoDb == null || chamadoDb.UsuarioId != GetUsuarioId())
                    return RedirectToAction("Index");

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



        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                var chamado = _chamadoDAO.Consulta(id);
                if (chamado == null || chamado.UsuarioId != GetUsuarioId())
                    return RedirectToAction("Index");

                ViewBag.Operacao = "D";
                return View("Form",chamado);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }


        [HttpPost]
        public IActionResult ExcluirConfirmado(int id)
        {
            if (!IsUserLogged())
                return RedirectToAction("Index", "Login");

            try
            {
                var chamado = _chamadoDAO.Consulta(id);
                if (chamado == null || chamado.UsuarioId != GetUsuarioId())
                    return RedirectToAction("Index");

                int usuarioIdLogado = GetUsuarioId();
                bool isAdmin = IsUserAdmin();

                if (!isAdmin && chamado.UsuarioId != usuarioIdLogado)
                    return  RedirectToAction("Index");

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

                ViewBag.Operacao = "R";
                return View("Form", chamado);
            }
            catch (Exception ex)
            {
                return View("Error", new ErrorViewModel(ex.Message));
            }
        }


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
