using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ProjetoPBL.Controllers
{
    public class SensorController : PadraoController<SensorViewModel>
    {
        public SensorController() { dao = new SensorDAO(); }

        private async Task ValidarDadosFiware(SensorViewModel model, string operacao)
        {
            if (!await HelperFiwareDAO.VerificarServer(HelperFiwareDAO.host))
            {
                ModelState.AddModelError("nomeSensor", "Servidor FIWARE indisponível no momento.");
            }

            if (ModelState.IsValid && operacao == "I")
            {
                await HelperFiwareDAO.CriarLamp(HelperFiwareDAO.host, model.nomeSensor);
            }
        }

        protected override void AdicionarViewbagsForm()
        {
            // Se precisar popular combos
        }

        protected override void AdicionarViewbagsIndex()
        {
            // Se necessário para listagem
        }

        protected override void ValidarDados(SensorViewModel model, string operacao)
        {
            base.ValidarDados(model, operacao);

            SensorDAO sDAO = new SensorDAO();

            if (operacao == "I" && sDAO.VerificarSensoresRepetidos(model.nomeSensor) > 0)
            {
                ModelState.AddModelError("nomeSensor", "Já existe um sensor com esse nome.");
            }

            if (string.IsNullOrWhiteSpace(model.descricaoSensor))
                ModelState.AddModelError("descricaoSensor", "A descrição é obrigatória.");

            if (string.IsNullOrWhiteSpace(model.localInstalacao))
                ModelState.AddModelError("localInstalacao", "Informe o local de instalação.");

            if (model.valorInstalacao < 0)
                ModelState.AddModelError("valorInstalacao", "O valor de instalação não pode ser negativo.");

            if (model.dataInstacao > DateTime.Now)
                ModelState.AddModelError("dataInstacao", "A data de instalação não pode estar no futuro.");

            if (ModelState.IsValid && operacao == "I")
                ValidarDadosFiware(model, operacao).GetAwaiter().GetResult();
        }

        public IActionResult TrocaMalha(string malha)
        {
            ViewBag.Malha = malha;
            return View("Dashboard", null);
        }

        public async Task<IActionResult> PegarUltimosDados()
        {
            string host = "4.228.64.5";
            string lampId = "03y";
            int lastN = 50;

            var leituras = await HelperFiwareDAO.VerificarDados(host, lampId, lastN);
            return Json(leituras);
        }
    }
}
