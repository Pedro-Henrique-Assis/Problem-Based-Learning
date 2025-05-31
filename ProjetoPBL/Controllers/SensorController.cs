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
        // ============================================================================
        // CONSTRUTOR
        // ============================================================================

        /// <summary>
        /// Inicializa o DAO específico para sensor e define se o ID será gerado automaticamente.
        /// </summary>
        public SensorController()
        {
            DAO = new SensorDAO();
            GeraProximoId = true;
        }

        // ============================================================================
        // VALIDAÇÕES
        // ============================================================================

        /// <summary>
        /// Valida os dados do formulário antes de inserir ou editar.
        /// Inclui validações específicas para sensor e integração com FIWARE.
        /// </summary>
        /// <param name="model">Objeto sensor</param>
        /// <param name="operacao">Operação atual ("I" ou "A")</param>
        protected override void ValidaDados(SensorViewModel model, string operacao)
        {
            base.ValidaDados(model, operacao);

            SensorDAO sDAO = new SensorDAO();

            // Verifica duplicidade no nome do sensor (apenas para inserção)
            if (operacao == "I" && sDAO.VerificarSensoresRepetidos(model.nomeSensor) > 0)
                ModelState.AddModelError("nomeSensor", "Já existe um sensor com esse nome.");

            // Validações obrigatórias
            if (string.IsNullOrWhiteSpace(model.descricaoSensor))
                ModelState.AddModelError("descricaoSensor", "A descrição é obrigatória.");

            if (string.IsNullOrWhiteSpace(model.localInstalacao))
                ModelState.AddModelError("localInstalacao", "Informe o local de instalação.");

            if (model.valorInstalacao < 0)
                ModelState.AddModelError("valorInstalacao", "O valor de instalação não pode ser negativo.");

            if (model.dataInstalacao > DateTime.Now)
                ModelState.AddModelError("dataInstalacao", "A data de instalação não pode estar no futuro.");

            // Integração com FIWARE somente se todas as validações passarem
            if (ModelState.IsValid && operacao == "I")
                ValidarDadosFiware(model, operacao).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Verifica disponibilidade do FIWARE e realiza a criação do sensor via API.
        /// </summary>
        /// <param name="model">Objeto sensor</param>
        /// <param name="operacao">Tipo de operação: "I" para inserir</param>
        private async Task ValidarDadosFiware(SensorViewModel model, string operacao)
        {
            if (!await HelperFiwareDAO.VerificarServer(HelperFiwareDAO.host))
            {
                ModelState.AddModelError("nomeSensor", "Servidor FIWARE indisponível no momento.");
                return;
            }

            if (operacao == "I")
                await HelperFiwareDAO.CriarSensorTemperatura(HelperFiwareDAO.host, model.nomeSensor);
        }

        // ============================================================================
        // AÇÕES DE FORMULÁRIO
        // ============================================================================

        /// <summary>
        /// Salva o sensor no banco de dados e no FIWARE,
        /// tratando tanto inserções quanto atualizações.
        /// </summary>
        /// <param name="model">Objeto sensor</param>
        /// <param name="operacao">Operação ("I" para inserir, "A" para atualizar)</param>
        /// <returns>Redirect ou View em caso de erro</returns>
        [HttpPost]
        public override IActionResult Save(SensorViewModel model, string operacao)
        {
            try
            {
                // Gera ID automaticamente, se necessário
                if (operacao == "I" && model.Id == 0 && GeraProximoId)
                    PreencheDadosParaView(operacao, model);

                // Valida os dados preenchidos
                ValidaDados(model, operacao);

                // Caso ocorram erros, retorna ao formulário com os dados
                if (!ModelState.IsValid)
                {
                    ViewBag.Operacao = operacao;
                    PreencheDadosParaView(operacao, model);
                    return View("Form", model);
                }

                // Executa operação no banco de dados
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

        /// <summary>
        /// Define valores padrão para o modelo, como a data de instalação atual.
        /// </summary>
        /// <param name="Operacao">Operação atual ("I" ou "A")</param>
        /// <param name="model">Objeto sensor</param>
        protected override void PreencheDadosParaView(string Operacao, SensorViewModel model)
        {
            base.PreencheDadosParaView(Operacao, model);

            if (Operacao == "I")
                model.dataInstalacao = DateTime.Now;
        }

        // ============================================================================
        // AÇÃO DE EXCLUSÃO
        // ============================================================================

        /// <summary>
        /// Exclui o sensor do banco de dados e também do FIWARE.
        /// </summary>
        /// <param name="id">ID do sensor a ser excluído</param>
        /// <returns>Redirect para a lista de sensores</returns>
        public override IActionResult Delete(int id)
        {
            try
            {
                // Recupera o sensor a ser excluído
                var sensor = DAO.Consulta(id);
                if (sensor == null)
                    return RedirectToAction("Index");

                // Remove o sensor do FIWARE
                HelperFiwareDAO.ExcluirSensor(HelperFiwareDAO.host, sensor.nomeSensor).GetAwaiter().GetResult();

                // Remove do banco de dados
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
