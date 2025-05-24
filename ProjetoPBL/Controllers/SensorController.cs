using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Threading.Tasks;

namespace ProjetoPBL.Controllers
{
    public class SensorController : PadraoController<SensorViewModel>
    {
        public SensorController()
        {
            DAO = new SensorDAO();
        }

        /// <summary>
        /// Valida os dados do sensor no FIWARE antes de inserir
        /// </summary>
        private async Task ValidarDadosFiware(SensorViewModel model, string operacao)
        {
            if (!await HelperFiwareDAO.VerificarServer(HelperFiwareDAO.host))
            {
                ModelState.AddModelError("nomeSensor", "Servidor FIWARE indisponível no momento.");
            }

            if (ModelState.IsValid && operacao == "I")
            {
                await HelperFiwareDAO.CriarSensorTemperatura(HelperFiwareDAO.host, model.nomeSensor);
            }
        }


        /// <summary>
        /// Valida os dados do formulário antes de inserir/editar
        /// </summary>
        protected override void ValidaDados(SensorViewModel model, string operacao)
        {
            base.ValidaDados(model, operacao);

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

            if (model.dataInstalacao > DateTime.Now)
                ModelState.AddModelError("dataInstalacao", "A data de instalação não pode estar no futuro.");

            if (ModelState.IsValid && operacao == "I")
                ValidarDadosFiware(model, operacao).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Endpoint que permite trocar a visualização entre tipos de malha
        /// </summary>
        //public IActionResult TrocaMalha(string malha)
        //{
        //    ViewBag.Malha = malha;
        //    return View("Dashboard", null);
        //}

        /// <summary>
        /// Retorna as últimas leituras de temperatura do sensor
        /// </summary>
        //public async Task<IActionResult> PegarUltimosDados()
        //{
        //    string host = "54.225.206.198";
        //    string sensorId = "03y"; // pode ser substituído por uma variável dinâmica
        //    int lastN = 50;

        //    var leituras = await HelperFiwareDAO.VerificarDados(host, sensorId, lastN);
        //    return Json(leituras);
        //}
    }
}
