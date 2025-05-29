using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjetoPBL.Models;
using System;

namespace ProjetoPBL.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard1()
        {
            return View("Dashboard1");
        }

        public IActionResult Dashboard2()
        {
            return View("Dashboard2");
        }

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
