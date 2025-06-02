using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using ProjetoPBL.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace ProjetoPBL.Services
{
    public class ColetorTemperaturaService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ColetorTemperaturaService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var controller = scope.ServiceProvider.GetRequiredService<TemperaturaController>();

                    await controller.ColetarSalvar(); // chama o método
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // executa a cada 1 minuto
            }
        }
    }
}
