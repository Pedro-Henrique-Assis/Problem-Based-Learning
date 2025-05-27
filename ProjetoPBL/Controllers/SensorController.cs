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
            GeraProximoId = true;
        }

        /// <summary>
        /// Valida os dados do sensor no FIWARE antes de inserir
        /// </summary>
        private async Task ValidarDadosFiware(SensorViewModel model, string operacao)
        {
            if (!await HelperFiwareDAO.VerificarServer(HelperFiwareDAO.host))
            {
                ModelState.AddModelError("nomeSensor", "Servidor FIWARE indisponível no momento.");
                return;
            }

            if (operacao == "I")
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

            // Validação externa só se as validações locais passarem
            if (ModelState.IsValid && operacao == "I")
                ValidarDadosFiware(model, operacao).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sobrescreve Save para adicionar mensagens com TempData
        /// </summary>
        [HttpPost]
        public override IActionResult Save(SensorViewModel model, string operacao)
        {
            try
            {
                if (operacao == "I" && model.Id == 0 && GeraProximoId)
                {
                    PreencheDadosParaView(operacao, model); // Garante que o ID seja gerado se não veio do GET
                }

                ValidaDados(model, operacao);

                if (!ModelState.IsValid)
                {
                    ViewBag.Operacao = operacao;
                    PreencheDadosParaView(operacao, model);
                    return View("Form", model);
                }

                if (operacao == "I")
                {
                    DAO.Insert(model);
                    TempData["Mensagem"] = "Sensor cadastrado com sucesso!";
                }
                else
                {
                    DAO.Update(model);
                    TempData["Mensagem"] = "Sensor atualizado com sucesso!";
                }

                return GetSaveRedirectAction(operacao);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }
    }
}
