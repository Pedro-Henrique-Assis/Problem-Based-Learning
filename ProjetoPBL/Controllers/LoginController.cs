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


        /// <summary>
        /// Processa a tentativa de login de um usuário.
        /// Este método recebe as credenciais (usuário e senha), verifica sua validade
        /// consultando o banco de dados através do UsuarioDAO e, se bem-sucedido,
        /// estabelece a sessão do usuário e o redireciona para a página inicial.
        /// Em caso de falha, exibe uma mensagem de erro na página de login.
        /// </summary>
        /// <param name="usuario">O nome de usuário ou login fornecido pelo usuário.</param>
        /// <param name="senha">A senha fornecida pelo usuário.</param>
        /// <returns>
        /// Retorna um <see cref="RedirectToActionResult"/> para a Action "Index" do Controller "Home"
        /// se o login for bem-sucedido.
        /// Retorna uma <see cref="ViewResult"/> para a View "Index" (provavelmente a página de login)
        /// com uma mensagem de erro no ViewBag se o login falhar.
        /// </returns>
        public IActionResult FazLogin(string usuario, string senha)
        {
            var usuarioDAO = new UsuarioDAO();
            UsuarioViewModel u = usuarioDAO.Consulta(usuario, senha);

            // Verifica se o usuário foi encontrado
            if (u != null)
            {
                // Se o usuário for encontrado (login bem-sucedido):
                // Define as informações do usuário na sessão.
                HttpContext.Session.SetString("Logado", "true");
                HttpContext.Session.SetString("IdUsuario", u.Id.ToString()); // Guarda o ID do usuário.
                HttpContext.Session.SetString("NomeUsuario", u.Nome);
                HttpContext.Session.SetString("IsAdmin", u.IsAdmin.ToString());
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Se o usuário não for encontrado (login falhou):
                // Define uma mensagem de erro que será exibida na View.
                ViewBag.Erro = "Usuário ou senha inválidos!";
                return View("Index");
            }
        }


        /// <summary>
        /// Realiza o logoff do usuário no sistema.
        /// Este método limpa todas as informações armazenadas na sessão do usuário atual
        /// e, em seguida, redireciona o usuário para a página de login (ou a página Index
        /// do controller atual, que é assumido ser o LoginController).
        /// </summary>
        /// <returns>
        /// Retorna um <see cref="RedirectToActionResult"/> que redireciona o usuário para a
        /// Action "Index" (presumivelmente a página de login).
        /// </returns>
        public IActionResult LogOff()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
