using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;

namespace ProjetoPBL.Controllers
{
    public class DashboardController : Controller
    {
        /// <summary>
        /// Executado antes de qualquer ação do controller.
        /// Verifica se o usuário está logado, redirecionando para a página de login caso não esteja.
        /// Caso esteja logado, define ViewBag.Logado como true para uso nas views.
        /// </summary>
        /// <param name="context">Contexto da ação executando.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!HelperControllers.VerificaUserLogado(HttpContext.Session))
                context.Result = RedirectToAction("Index", "Login"); // Redireciona para login se não estiver logado
            else
            {
                ViewBag.Logado = true; // Usuário logado, informa a view
                base.OnActionExecuting(context);
            }
        }

        /// <summary>
        /// Action que exibe o Dashboard1.
        /// Carrega os dados de temperatura e calcula parâmetros do sistema (ganho, constante de tempo tau, tempo real e valor alvo 63,2%).
        /// Passa esses valores para a View via ViewBag e retorna a lista de dados para exibição.
        /// </summary>
        /// <returns>View "Dashboard1" com dados de temperatura e parâmetros calculados.</returns>
        public IActionResult Dashboard1()
        {
            var dao = new TemperaturaDAO();
            List<TemperaturaViewModel> dados = dao.Listar();

            var (ganhoK, tau, tempoReal, alvo632) = dao.CalcularParametros();
            ViewBag.GanhoK = ganhoK;
            ViewBag.ConstanteTau = tau;
            ViewBag.RecvTimeTau = tempoReal?.ToString("HH:mm");
            ViewBag.Alvo632 = alvo632;

            return View("Dashboard1", dados);
        }

        /// <summary>
        /// Action que exibe o Dashboard2.
        /// Fornece uma lista fixa de dados para regressão linear e passa coeficientes, desvios e erros para a View via ViewBag.
        /// </summary>
        /// <returns>View "Dashboard2" com dados estáticos para análise de regressão.</returns>
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
    }
}
