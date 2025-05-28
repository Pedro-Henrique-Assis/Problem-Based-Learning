using Microsoft.AspNetCore.Mvc;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoPBL.Controllers
{
    public class ConsultaSensorController : Controller
    {
        private readonly SensorDAO _sensorDAO;

        public ConsultaSensorController()
        {
            _sensorDAO = new SensorDAO();
        }

        public IActionResult Index()
        {
            return View("ConsultaAvancadaSensor");
        }

        public IActionResult ObtemDadosConsulta(string local, decimal? valorMin, decimal? valorMax, DateTime? dataInicial, DateTime? dataFinal)
        {
            try
            {
                var lista = _sensorDAO.ConsultaAvancada(local, valorMin, valorMax, dataInicial, dataFinal);
                return PartialView("pvGridSensor", lista);
            }
            catch (Exception ex)
            {
                return Json(new { erro = true, msg = ex.Message });
            }
        }
    }
}
