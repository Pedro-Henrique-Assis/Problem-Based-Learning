using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;

namespace ProjetoPBL.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);
            return View();
        }

        public IActionResult FazLogin(string usuario, string senha)
        {
            var usuarioDAO = new UsuarioDAO();
            UsuarioViewModel u = usuarioDAO.Consulta(usuario, senha);

            if (u != null)
            {
                HttpContext.Session.SetString("Logado", "true");
                HttpContext.Session.SetString("IdUsuario", u.Id.ToString()); // Guarda o ID do usuário.
                HttpContext.Session.SetString("NomeUsuario", u.Nome);
                HttpContext.Session.SetString("IsAdmin", u.IsAdmin.ToString());
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Erro = "Usuário ou senha inválidos!";
                return View("Index");
            }
        }
        
        public IActionResult LogOff()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
