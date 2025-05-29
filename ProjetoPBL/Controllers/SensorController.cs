using Microsoft.AspNetCore.Mvc;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Threading.Tasks;

namespace ProjetoPBL.Controllers
{
    /// <summary>
    /// Controller responsável pelo gerenciamento dos sensores,
    /// incluindo operações CRUD e integração com o FIWARE.
    /// </summary>
    public class SensorController : PadraoController<SensorViewModel>
    {
        /// <summary>
        /// Construtor do controller que inicializa o DAO específico e configura geração automática de ID.
        /// </summary>
        public SensorController()
        {
            DAO = new SensorDAO();
            GeraProximoId = true;
        }

        /// <summary>
        /// Valida dados e realiza integração com FIWARE antes de inserir um sensor.
        /// Verifica se o servidor FIWARE está disponível e cria o sensor via API.
        /// </summary>
        /// <param name="model">Objeto sensor a ser validado</param>
        /// <param name="operacao">Tipo de operação: "I" para inserir</param>
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
        /// Valida os dados do formulário antes de inserir ou editar.
        /// Inclui validações específicas para sensor e chama validação externa FIWARE.
        /// </summary>
        /// <param name="model">Objeto sensor</param>
        /// <param name="operacao">Operação atual ("I" ou "A")</param>
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
        /// Sobrescreve o método Save para inserir ou atualizar sensor,
        /// adicionando mensagens de sucesso via TempData.
        /// </summary>
        /// <param name="model">Objeto sensor</param>
        /// <param name="operacao">Operação ("I" para inserir, "A" para atualizar)</param>
        /// <returns>Redirect ou View em caso de erro</returns>
        [HttpPost]
        public override IActionResult Save(SensorViewModel model, string operacao)
        {
            try
            {
                // Gera ID automaticamente se necessário na inserção
                if (operacao == "I" && model.Id == 0 && GeraProximoId)
                {
                    PreencheDadosParaView(operacao, model);
                }

                // Valida dados do sensor
                ValidaDados(model, operacao);

                // Se houver erro de validação, retorna para o formulário
                if (!ModelState.IsValid)
                {
                    ViewBag.Operacao = operacao;
                    PreencheDadosParaView(operacao, model);
                    return View("Form", model);
                }

                // Executa inserção ou atualização no banco
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

                // Redireciona após salvar
                return GetSaveRedirectAction(operacao);
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }

        /// <summary>
        /// Preenche dados padrão para as views,
        /// por exemplo definindo data de instalação na inserção.
        /// </summary>
        /// <param name="Operacao">Operação atual ("I" ou "A")</param>
        /// <param name="model">Objeto sensor</param>
        protected override void PreencheDadosParaView(string Operacao, SensorViewModel model)
        {
            base.PreencheDadosParaView(Operacao, model);

            if (Operacao == "I")
                model.dataInstalacao = DateTime.Now;
        }

        /// <summary>
        /// Sobrescreve o método Delete para também excluir o sensor no FIWARE
        /// antes de apagar localmente.
        /// </summary>
        /// <param name="id">ID do sensor a ser excluído</param>
        /// <returns>Redirect para lista ou view de erro</returns>
        public override IActionResult Delete(int id)
        {
            try
            {
                // Consulta sensor para obter dados
                var sensor = DAO.Consulta(id);
                if (sensor == null)
                    return RedirectToAction("Index");

                // Exclui sensor no FIWARE (chamada síncrona)
                HelperFiwareDAO.ExcluirSensor(HelperFiwareDAO.host, sensor.nomeSensor).GetAwaiter().GetResult();

                // Exclui sensor localmente no banco de dados
                DAO.Delete(id);

                TempData["Mensagem"] = "Sensor excluído com sucesso, incluindo o FIWARE.";

                return RedirectToAction("Index");
            }
            catch (Exception erro)
            {
                return View("Error", new ErrorViewModel(erro.ToString()));
            }
        }
    }
}
