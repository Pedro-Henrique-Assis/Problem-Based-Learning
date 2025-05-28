// HelperFiwareDAO.cs
// Classe responsável pela integração do sistema com o FIWARE
// Realiza operações como verificação de servidor, leitura de sensores, provisionamento de dispositivos e subscrição de dados

using ProjetoPBL.Models;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;

namespace ProjetoPBL.DAO
{
    public static class HelperFiwareDAO
    {
        // IP padrão do host FIWARE
        public static string host = "54.225.206.198";

        /// <summary>
        /// Verifica se o servidor FIWARE está online
        /// </summary>
        public static async Task<bool> VerificarServer(string host)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var response = await client.GetAsync($"http://{host}:4041/iot/about");

                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        /// <summary>
        /// Retorna as últimas N leituras de temperatura do sensor
        /// </summary>
        public static async Task<List<LeituraViewModel>> VerificarDados(string host, string sensor, int n)
        {
            List<LeituraViewModel> leituras = new List<LeituraViewModel>();

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"http://{host}:8666/STH/v1/contextEntities/type/TemperatureSensor/id/urn:ngsi-ld:TemperatureSensor:{sensor}/attributes/temperature?lastN={n}");
            request.Headers.Add("fiware-service", "smart");
            request.Headers.Add("fiware-servicepath", "/");

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(content);
                var contexto = doc.RootElement.GetProperty("contextResponses");

                foreach (var contextResponse in contexto.EnumerateArray())
                {
                    var contextElement = contextResponse.GetProperty("contextElement");
                    if (contextElement.TryGetProperty("attributes", out var attributes))
                    {
                        foreach (var attribute in attributes.EnumerateArray())
                        {
                            if (attribute.GetProperty("name").GetString() == "temperature")
                            {
                                var values = attribute.GetProperty("values");
                                foreach (var value in values.EnumerateArray())
                                {
                                    DateTime recvTime = DateTime.Parse(value.GetProperty("recvTime").GetString());
                                    float attrValue = value.GetProperty("attrValue").GetSingle();
                                    leituras.Add(new LeituraViewModel(attrValue, recvTime));
                                }
                            }
                        }
                    }
                }
            }
            return leituras;
        }

        /// <summary>
        /// Retorna a última leitura de temperatura do sensor
        /// </summary>
        public static async Task<LeituraViewModel> Ler(string host, string sensor)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"http://{host}:1026/v2/entities/urn:ngsi-ld:TemperatureSensor:{sensor}/attrs/temperature");
            request.Headers.Add("fiware-service", "smart");
            request.Headers.Add("fiware-servicepath", "/");
            request.Headers.Add("accept", "application/json");

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(content);
                float temp = doc.RootElement.GetProperty("value").GetSingle();
                string dataString = doc.RootElement.GetProperty("metadata").GetProperty("TimeInstant").GetProperty("value").ToString();
                DateTime data = DateTime.Parse(dataString);
                return new LeituraViewModel(temp, data);
            }
            return new LeituraViewModel();
        }

        /// <summary>
        /// Executa o ciclo completo de criação de um sensor de temperatura
        /// </summary>
        public static async Task CriarSensorTemperatura(string host, string sensor)
        {
            await ProvisionarSensor(host, sensor);
            await RegistrarSensor(host, sensor);
            await AssinarSensor(host, sensor);
        }

        /// <summary>
        /// Provisona um novo sensor no IoT Agent
        /// </summary>
        private static async Task ProvisionarSensor(string host, string sensor)
        {
            using var client = new HttpClient();
            var url = $"http://{host}:4041/iot/devices";

            var body = new
            {
                devices = new[]
                {
                    new
                    {
                        device_id = sensor, // usa o nome exato do sensor
                        entity_name = $"urn:ngsi-ld:TemperatureSensor:{sensor}",
                        entity_type = "TemperatureSensor",
                        protocol = "PDI-IoTA-UltraLight",
                        transport = "MQTT",
                        attributes = new[]
                        {
                            new { object_id = "t", name = "temperature", type = "float" }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("fiware-service", "smart");
            content.Headers.Add("fiware-servicepath", "/");

            await client.PostAsync(url, content);
        }

        /// <summary>
        /// Registra o sensor de temperatura no Orion Context Broker
        /// </summary>
        private static async Task RegistrarSensor(string host, string sensor)
        {
            using var client = new HttpClient();
            var url = $"http://{host}:1026/v2/registrations";

            var body = new
            {
                description = "Sensor de Temperatura",
                dataProvided = new
                {
                    entities = new[]
                    {
                        new { id = $"urn:ngsi-ld:TemperatureSensor:{sensor}", type = "TemperatureSensor" }
                    },
                    attrs = new[] { "temperature" }
                },
                provider = new
                {
                    http = new { url = $"http://{host}:4041" },
                    legacyForwarding = true
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("fiware-service", "smart");
            content.Headers.Add("fiware-servicepath", "/");

            await client.PostAsync(url, content);
        }

        /// <summary>
        /// Subscrição do sensor para envio automático de notificações ao STH-Comet
        /// </summary>
        private static async Task AssinarSensor(string host, string sensor)
        {
            using var client = new HttpClient();
            var url = $"http://{host}:1026/v2/subscriptions";

            var body = new
            {
                description = "Subscrição de Temperatura",
                subject = new
                {
                    entities = new[]
                    {
                        new { id = $"urn:ngsi-ld:TemperatureSensor:{sensor}", type = "TemperatureSensor" }
                    },
                    condition = new { attrs = new[] { "temperature" } }
                },
                notification = new
                {
                    http = new { url = $"http://{host}:8666/notify" },
                    attrs = new[] { "temperature" },
                    attrsFormat = "legacy"
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.Add("fiware-service", "smart");
            content.Headers.Add("fiware-servicepath", "/");

            await client.PostAsync(url, content);
        }

        /// <summary>
        /// Exclui o sensor do FIWARE Orion Context Broker e IoT Agent
        /// </summary>
        /// <param name="host">Host do FIWARE</param>
        /// <param name="sensor">Identificador do sensor</param>
        /// <returns>Task assíncrona</returns>
        public static async Task ExcluirSensor(string host, string sensor)
        {
            using var client = new HttpClient();

            // URL para excluir dispositivo no IoT Agent
            var urlIoTAgent = $"http://{host}:4041/iot/devices/{sensor}";
            var requestIoTAgent = new HttpRequestMessage(HttpMethod.Delete, urlIoTAgent);
            requestIoTAgent.Headers.Add("fiware-service", "smart");
            requestIoTAgent.Headers.Add("fiware-servicepath", "/");
            var responseIoTAgent = await client.SendAsync(requestIoTAgent);
            responseIoTAgent.EnsureSuccessStatusCode();

            // URL para excluir entidade no Orion Context Broker
            var urlOrion = $"http://{host}:1026/v2/entities/urn:ngsi-ld:TemperatureSensor:{sensor}";
            var requestOrion = new HttpRequestMessage(HttpMethod.Delete, urlOrion);
            requestOrion.Headers.Add("fiware-service", "smart");
            requestOrion.Headers.Add("fiware-servicepath", "/");
            var responseOrion = await client.SendAsync(requestOrion);
            responseOrion.EnsureSuccessStatusCode();
        }
    }
}
