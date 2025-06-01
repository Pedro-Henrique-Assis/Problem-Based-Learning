using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjetoPBL.Controllers
{
    public class SobreController : Controller
    {
        public IActionResult Index()
        {
            return View();
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
