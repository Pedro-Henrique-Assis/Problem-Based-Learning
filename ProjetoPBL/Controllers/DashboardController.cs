using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;

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
            var dados = new List<RegressaoViewModel>
            {
                new RegressaoViewModel { Temperatura = 30, Voltagem = 2.9613f, DesvioPadrao = 0.0401f, Peso = 622.23f },
                new RegressaoViewModel { Temperatura = 35, Voltagem = 3.5020f, DesvioPadrao = 0.0211f, Peso = 2236.58f },
                new RegressaoViewModel { Temperatura = 40, Voltagem = 3.9520f, DesvioPadrao = 0.0318f, Peso = 988.57f },
                new RegressaoViewModel { Temperatura = 45, Voltagem = 4.4840f, DesvioPadrao = 0.0150f, Peso = 4464.28f },
                new RegressaoViewModel { Temperatura = 50, Voltagem = 4.9647f, DesvioPadrao = 0.0161f, Peso = 3839.59f },
                new RegressaoViewModel { Temperatura = 55, Voltagem = 5.4553f, DesvioPadrao = 0.0285f, Peso = 1232.20f }
            };

            ViewBag.B1 = 0.0987f;
            ViewBag.B0 = 0.0344f;
            ViewBag.DesvioPadraoPonderado = 0.6139f;
            ViewBag.ErroB1 = 0.0010f;
            ViewBag.ErroB0 = 0.0454f;

            return View("Dashboard2", dados);
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

