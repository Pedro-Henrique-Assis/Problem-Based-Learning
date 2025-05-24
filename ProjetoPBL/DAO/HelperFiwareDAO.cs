// HelperFiwareDAO.cs
// Classe responsável pela integração do sistema com o FIWARE
// Realiza operações como verificação de servidor, leitura de sensores, provisionamento de dispositivos e subscrição de dados

using RestSharp;
using SitePBL.Models;
using System.Text.Json;

namespace SitePBL.DAO
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
                var options = new RestClientOptions($"http://{host}:4041") { MaxTimeout = -1 };
                var client = new RestClient(options);
                var request = new RestRequest("/iot/about", Method.Get);
                var response = await client.ExecuteAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        /// <summary>
        /// Retorna as últimas N leituras de temperatura do sensor
        /// </summary>
        public static async Task<List<LeituraViewModel>> VerificarDados(string host, string sensor, int n)
        {
            List<LeituraViewModel> leituras = new();

            var options = new RestClientOptions($"http://{host}:8666") { MaxTimeout = -1 };
            var client = new RestClient(options);
            var request = new RestRequest($"/STH/v1/contextEntities/type/TemperatureSensor/id/urn:ngsi-ld:TemperatureSensor:{sensor}/attributes/temperature?lastN={n}", Method.Get);
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var doc = JsonDocument.Parse(response.Content);
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
            var options = new RestClientOptions($"http://{host}:1026") { MaxTimeout = -1 };
            var client = new RestClient(options);
            var request = new RestRequest($"/v2/entities/urn:ngsi-ld:TemperatureSensor:{sensor}/attrs/temperature", Method.Get);
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");
            request.AddHeader("accept", "application/json");

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var doc = JsonDocument.Parse(response.Content);
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
            var options = new RestClientOptions($"http://{host}:4041") { MaxTimeout = -1 };
            var client = new RestClient(options);
            var request = new RestRequest("/iot/devices", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");

            var body = new
            {
                devices = new[]
                {
                    new
                    {
                        device_id = $"tempsensor{sensor}",
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

            string jsonBody = JsonSerializer.Serialize(body);
            request.AddStringBody(jsonBody, DataFormat.Json);
            await client.ExecuteAsync(request);
        }

        /// <summary>
        /// Registra o sensor de temperatura no Orion Context Broker
        /// </summary>
        private static async Task RegistrarSensor(string host, string sensor)
        {
            var options = new RestClientOptions($"http://{host}:1026") { MaxTimeout = -1 };
            var client = new RestClient(options);
            var request = new RestRequest("/v2/registrations", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");

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

            string jsonBody = JsonSerializer.Serialize(body);
            request.AddStringBody(jsonBody, DataFormat.Json);
            await client.ExecuteAsync(request);
        }

        /// <summary>
        /// Subscrição do sensor para envio automático de notificações ao STH-Comet
        /// </summary>
        private static async Task AssinarSensor(string host, string sensor)
        {
            var options = new RestClientOptions($"http://{host}:1026") { MaxTimeout = -1 };
            var client = new RestClient(options);
            var request = new RestRequest("/v2/subscriptions", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("fiware-service", "smart");
            request.AddHeader("fiware-servicepath", "/");

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

            string jsonBody = JsonSerializer.Serialize(body);
            request.AddStringBody(jsonBody, DataFormat.Json);
            await client.ExecuteAsync(request);
        }
    }
}
