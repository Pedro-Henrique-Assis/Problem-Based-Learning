using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjetoPBL.Models;
using Microsoft.AspNetCore.Http;

namespace ProjetoPBL.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Verifica se o usuário está logado e, se sim, permite o acesso à Home
        /// </summary>
        /// <returns>Retorna a view Home/Index.cshtml</returns>
        public IActionResult Index()
        {
            ViewBag.Logado = HelperControllers.VerificaUserLogado(HttpContext.Session);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        /// <summary>
        /// Sobrescreve o método OnActionExecuting para realizar a verificação de autenticação
        /// antes que qualquer Action (método de controller) seja executada.
        /// Este método é um filtro de ação que é invocado automaticamente pelo ASP.NET Core
        /// antes da execução da Action do controller.
        /// </summary>
        /// <param name="context">
        /// O objeto ActionExecutingContext que contém informações sobre a Action que está prestes a ser executada,
        /// incluindo o HttpContext (e, consequentemente, a sessão).
        /// </param>
        /// <remarks>
        /// Se o usuário não estiver logado (verificado através de `HelperControllers.VerificaUserLogado`),
        /// a execução da Action atual é interrompida e o usuário é redirecionado para a página de Login.
        /// Caso contrário, uma variável `ViewBag.Logado` é definida como `true` (para ser usada nas Views)
        /// e a execução da Action prossegue normalmente chamando o método base.
        /// </remarks>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!HelperControllers.VerificaUserLogado(HttpContext.Session))
                context.Result = RedirectToAction("Index", "Login");
            else
            {
                ViewBag.Logado = true;
                base.OnActionExecuting(context);
            }
        }
    }
}
