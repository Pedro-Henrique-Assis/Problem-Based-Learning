using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoPBL.DAO;

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
            bool existe = false;
            var usuarioDAO = new UsuarioDAO();
            var lista = usuarioDAO.Listagem();

            foreach(var usuarioCadastrado in lista)
            {
                if (usuarioCadastrado.LoginUsuario == usuario && usuarioCadastrado.Senha == senha)
                    existe = true;
            }

            if (existe)
            {
                HttpContext.Session.SetString("Logado", "true");
                return RedirectToAction("index", "Home");
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
