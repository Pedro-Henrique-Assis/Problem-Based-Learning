using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using ProjetoPBL.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace ProjetoPBL.Services
{
    /// <summary>
    /// Serviço em segundo plano (background service) responsável por coletar dados de temperatura periodicamente.
    /// </summary>
    public class ColetorTemperaturaService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Construtor que recebe o provedor de serviços para permitir a injeção de dependências no escopo do serviço.
        /// </summary>
        /// <param name="serviceProvider">Provedor de serviços para criação de escopos.</param>
        public ColetorTemperaturaService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Método executado em loop enquanto o serviço estiver ativo.
        /// A cada 3 segundos, cria um escopo para resolver o controller TemperaturaController e chama o método de coleta e salvamento.
        /// </summary>
        /// <param name="stoppingToken">Token para cancelar a execução do serviço.</param>
        /// <returns>Task assíncrona que representa a execução contínua do serviço.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Cria um escopo para resolver dependências com tempo de vida curto
                using (var scope = _serviceProvider.CreateScope())
                {
                    var controller = scope.ServiceProvider.GetRequiredService<TemperaturaController>();

                    // Chama o método assíncrono que realiza a coleta e salvamento da temperatura
                    await controller.ColetarSalvar();
                }

                // Aguarda 3 segundos antes da próxima execução, respeitando o token de cancelamento
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }
}
