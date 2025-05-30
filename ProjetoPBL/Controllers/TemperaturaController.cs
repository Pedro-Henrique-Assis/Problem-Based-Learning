using Microsoft.AspNetCore.Mvc;
using ProjetoPBL.DAO;
using ProjetoPBL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjetoPBL.Controllers
{
    public class TemperaturaController : Controller
    {
        private readonly TemperaturaDAO dao = new TemperaturaDAO();

        private readonly string fiwareUrl = "http://54.225.206.198:8666/STH/v1/contextEntities/type/Temperature/id/urn:ngsi-ld:Temperature:001/attributes/temperature?lastN=100";

        // Método público para ser chamado (ex: por Hangfire) para coletar e salvar dados
        public async Task<IActionResult> ColetarSalvar()
        {
            var dados = await BuscarTemperaturasFIWARE();

            if (dados != null && dados.Any())
            {
                foreach (var temp in dados)
                {
                    dao.InserirSeNaoExiste(new TemperaturaViewModel
                    {
                        SensorId = "lamp001",
                        RecvTime = temp.recvTime,
                        Temperature = temp.attrValue
                    });

                }
                return Ok($"Temperaturas salvas: {dados.Count}");
            }
            return BadRequest("Erro ao coletar temperaturas.");
        }


        // Consome a API FIWARE STH e retorna lista de valores
        private async Task<List<Value>> BuscarTemperaturasFIWARE()
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Fiware-Service", "smart");
            client.DefaultRequestHeaders.Add("Fiware-ServicePath", "/");

            var response = await client.GetAsync(fiwareUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro ao buscar FIWARE: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"JSON recebido: {json}");

            var sthResponse = JsonSerializer.Deserialize<StHResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var values = sthResponse?.contextResponses?.FirstOrDefault()?
                            .contextElement?.attributes?.FirstOrDefault()?
                            .values;

            return values;
        }



        // Classes para desserializar o JSON do STH
        private class StHResponse
        {
            public List<ContextResponse> contextResponses { get; set; }
        }

        private class ContextResponse
        {
            public ContextElement contextElement { get; set; }
        }

        private class ContextElement
        {
            public string id { get; set; }
            public string type { get; set; }
            public List<Attribute> attributes { get; set; }
        }

        private class Attribute
        {
            public string name { get; set; }
            public List<Value> values { get; set; }
        }

        public class Value
        {
            public DateTime recvTime { get; set; }
            public float attrValue { get; set; }
        }

        // Método para listar os dados do banco (exemplo para o frontend consumir)
        public IActionResult Listar()
        {
            var lista = dao.Listar();
            return Json(lista);
        }
    }
}
