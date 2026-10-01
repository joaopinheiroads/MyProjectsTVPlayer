using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TVPlayerSite.Disparos
{
    public class DisparosDigisac : IDisparoMensagem
    {
        private readonly string _baseUrl;
        private readonly string _token;
        private readonly string _serviceId;
        private readonly HttpMessageHandler _handler;
        private readonly int _pollDelayMs;
        private readonly int _pollTentativas;

      
        public DisparosDigisac(string baseUrl, string token, string serviceId)
            : this(baseUrl, token, serviceId, null) { }

      
        public DisparosDigisac(string baseUrl, string token, string serviceId, HttpMessageHandler handler, int pollDelayMs = 600, int pollTentativas = 6)
        {
            _baseUrl = baseUrl;
            _token = token;
            _serviceId = serviceId;
            _handler = handler;
            _pollDelayMs = pollDelayMs;
            _pollTentativas = pollTentativas;
        }

  
        private HttpClient CriarHttpClient()
            => _handler != null ? new HttpClient(_handler, disposeHandler: false) : new HttpClient();

      
        public async Task<EnvioMensagemResultado> EnviarMensagemAsync(string number, string message)
        {
            if (string.IsNullOrEmpty(number))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "Número de telefone inválido. Inclua o código do país."
                };
            }

            var payload = new
            {
                text = message,
                number = number,
                serviceId = _serviceId,
                origin = "bot",
                dontOpenTicket = true
            };

            return await EnviarPayloadAsync(payload);
        }

        
        public async Task<EnvioMensagemResultado> EnviarTemplateAsync(string number, string templateId, bool comBotaoCopyCode, params string[] parametros)
        {
            if (string.IsNullOrEmpty(number))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "Número de telefone inválido. Inclua o código do país."
                };
            }

            if (string.IsNullOrEmpty(templateId))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "templateId (hsmId) não informado na chamada."
                };
            }

            parametros = parametros ?? new string[0];

            var componentes = new System.Collections.Generic.List<object>();

      
            if (parametros.Length > 0)
            {
                
                var bodyParams = parametros.Select(p => new { type = "text", text = p }).ToArray();
                componentes.Add(new { type = "body", parameters = bodyParams });
            }

            
            if (comBotaoCopyCode && parametros.Length > 0)
            {
                componentes.Add(new
                {
                    type = "button",
                    sub_type = "url",
                    index = 0,
                    parameters = new[] { new { type = "text", text = parametros[0] } }
                });
            }

            var payload = new
            {
                type = "chat",
                number = number,
                serviceId = _serviceId,
                hsmId = templateId,
                parameters = componentes,
                files = new object[0],
                uploadingFiles = false,
                replyTo = (object)null,
                file = new { }
            };

            return await EnviarPayloadAsync(payload);
        }

       
        public async Task<EnvioMensagemResultado> EnviarTemplateComBotaoUrlAsync(string number, string templateId, string urlButtonParam, int urlButtonIndex = 0, params string[] parametrosBody)
        {
            if (string.IsNullOrEmpty(number))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "Número de telefone inválido. Inclua o código do país."
                };
            }

            if (string.IsNullOrEmpty(templateId))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "templateId (hsmId) não informado na chamada."
                };
            }

            if (string.IsNullOrEmpty(urlButtonParam))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "urlButtonParam (valor do {{1}} do botão de URL) não informado."
                };
            }

            parametrosBody = parametrosBody ?? new string[0];

            var componentes = new System.Collections.Generic.List<object>();

            if (parametrosBody.Length > 0)
            {
                var bodyParams = parametrosBody.Select(p => new { type = "text", text = p }).ToArray();
                componentes.Add(new { type = "body", parameters = bodyParams });
            }

            componentes.Add(new
            {
                type = "button",
                sub_type = "url",
                index = urlButtonIndex,
                parameters = new[] { new { type = "text", text = urlButtonParam } }
            });

            var payload = new
            {
                type = "chat",
                number = number,
                serviceId = _serviceId,
                hsmId = templateId,
                parameters = componentes,
                files = new object[0],
                uploadingFiles = false,
                replyTo = (object)null,
                file = new { }
            };

            return await EnviarPayloadAsync(payload);
        }

        public async Task<EnvioMensagemResultado> EnviarTemplateComImagemAsync(string number, string templateId, string urlImagem, params string[] parametrosBody)
        {
            if (string.IsNullOrEmpty(number))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "Número de telefone inválido. Inclua o código do país."
                };
            }

            if (string.IsNullOrEmpty(templateId))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "templateId (hsmId) não informado na chamada."
                };
            }

            if (string.IsNullOrEmpty(urlImagem))
            {
                return new EnvioMensagemResultado
                {
                    Sucesso = false,
                    Erro = "urlImagem (imagem do cabeçalho do template) não informada."
                };
            }

            parametrosBody = parametrosBody ?? new string[0];

            var componentes = new System.Collections.Generic.List<object>
            {
                new
                {
                    type = "header",
                    parameters = new[] { new { type = "image", image = new { link = urlImagem } } }
                }
            };

            if (parametrosBody.Length > 0)
            {
                var bodyParams = parametrosBody.Select(p => new { type = "text", text = p }).ToArray();
                componentes.Add(new { type = "body", parameters = bodyParams });
            }

            var payload = new
            {
                type = "chat",
                number = number,
                serviceId = _serviceId,
                hsmId = templateId,
                parameters = componentes,
                files = new object[0],
                uploadingFiles = false,
                replyTo = (object)null,
                file = new { }
            };

            return await EnviarPayloadAsync(payload);
        }

        private async Task<EnvioMensagemResultado> EnviarPayloadAsync(object payload)
        {
            try
            {
                using (var client = CriarHttpClient())
                {
                    string json = JsonConvert.SerializeObject(payload);

                    var baseUrl = _baseUrl.TrimEnd('/');
                    if (baseUrl.EndsWith("/api/v1", StringComparison.OrdinalIgnoreCase))
                        baseUrl = baseUrl.Substring(0, baseUrl.Length - "/api/v1".Length);

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(baseUrl + "/api/v1/messages"),
                        Content = new StringContent(json, Encoding.UTF8, "application/json")
                    };
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    var requestUri = baseUrl + "/api/v1/messages";

                    using (var response = await client.SendAsync(request))
                    {
                        string body = await response.Content.ReadAsStringAsync();

                        // Log de diagnóstico: grava a requisição EXATA (estrutura/índice dos
                        // componentes) e a resposta crua do Digisac. Necessário porque o 2xx
                        // do Digisac esconde erros assíncronos da Meta/Gupshup ("falta de
                        // parâmetro") que só aparecem no painel, nunca no log de WhatsApp.
                        RegistrarLogDigisac(requestUri, json, (int)response.StatusCode, body);

                        if (!response.IsSuccessStatusCode)
                        {
                            return new EnvioMensagemResultado
                            {
                                Sucesso = false,
                                Erro = $"Digisac retornou {(int)response.StatusCode}: {body}"
                            };
                        }

                        // 2xx do Digisac = "aceito", NÃO "entregue". A Meta valida de forma
                        // ASSÍNCRONA e o veredito real chega por webhook em ~1-2s (ex.: a
                        // rejeição 131008 de botão de URL sem parâmetro). Sem confirmar o
                        // status, uma rejeição da Meta era registrada como falso SUCESSO no
                        // log. Aqui puxamos o status real da mensagem antes de reportar.
                        string mensagemId = ExtrairId(body);
                        if (string.IsNullOrEmpty(mensagemId))
                        {
                            // Sem id não há como confirmar; preserva o comportamento anterior.
                            return new EnvioMensagemResultado { Sucesso = true };
                        }

                        return await ConfirmarEntregaAsync(client, baseUrl, mensagemId);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                RegistrarLogDigisac("(exceção antes da resposta)", null, -1, ex.ToString());
                return new EnvioMensagemResultado { Sucesso = false, Erro = ex.Message };
            }
            catch (Exception ex)
            {
                RegistrarLogDigisac("(exceção antes da resposta)", null, -1, ex.ToString());
                return new EnvioMensagemResultado { Sucesso = false, Erro = ex.Message };
            }
        }

        // Confirma o status REAL da mensagem após o POST. O Digisac responde 2xx na hora
        // (data.ack = 0, "pendente"), mas o veredito da Meta chega ASSÍNCRONO por webhook
        // logo depois. Fazemos um poll curto:
        //   - data.ack == "error"    -> falha (retorna o motivo real da Meta);
        //   - data.ack numérico >= 1 -> entregue/lido (sucesso);
        //   - ainda pendente após o tempo-limite -> tratamos como aceito (sucesso "pendente"),
        //     para não transformar entrega legítima lenta (destinatário offline) em falha.
        // Rejeições da Meta voltam rápido (~1-2s), então o caso comum resolve em 1-2 tentativas.
        private async Task<EnvioMensagemResultado> ConfirmarEntregaAsync(HttpClient client, string baseUrl, string mensagemId)
        {
            var statusUrl = baseUrl + "/api/v1/messages/" + mensagemId;

            for (int tentativa = 0; tentativa < _pollTentativas; tentativa++)
            {
                await Task.Delay(_pollDelayMs);

                try
                {
                    var req = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(statusUrl)
                    };
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                    using (var resp = await client.SendAsync(req))
                    {
                        if (!resp.IsSuccessStatusCode)
                            continue;

                        var body = await resp.Content.ReadAsStringAsync();
                        var data = JObject.Parse(body)["data"];
                        var ack = data?["ack"]?.ToString();

                        if (ack == "error")
                        {
                            var erroObj = data["error"];
                            var msg = erroObj?["message"]?.ToString();
                            var det = erroObj?["error_data"]?["details"]?.ToString();
                            var motivo = string.IsNullOrEmpty(det) ? msg : $"{msg} — {det}";

                            // Registra o veredito real no log de diagnóstico do Digisac.
                            RegistrarLogDigisac(statusUrl, null, 200, body);

                            return new EnvioMensagemResultado
                            {
                                Sucesso = false,
                                Erro = $"Meta rejeitou a mensagem: {motivo}"
                            };
                        }

                        if (int.TryParse(ack, out int ackNum) && ackNum >= 1)
                        {
                            // 1=entregue, 2=lido, 3=reproduzido -> entrega confirmada.
                            return new EnvioMensagemResultado { Sucesso = true };
                        }

                        // ack 0 / "0" / vazio: ainda pendente -> continua tentando.
                    }
                }
                catch
                {
                    // Falha ao consultar status não pode derrubar o envio; tenta de novo.
                }
            }

            // Sem veredito definitivo dentro do tempo-limite: sem erro conhecido, tratamos
            // como aceito (entrega ainda pendente).
            return new EnvioMensagemResultado { Sucesso = true };
        }

        // Extrai o id da mensagem da resposta do POST (necessário para consultar o status).
        private static string ExtrairId(string body)
        {
            try
            {
                return JObject.Parse(body)["id"]?.ToString();
            }
            catch
            {
                return null;
            }
        }

       
        private static void RegistrarLogDigisac(string requestUri, string requestJson, int statusCode, string responseBody)
        {
            try
            {
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogsDigisac");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var filePath = Path.Combine(dir, $"Digisac_{DateTime.Now:yyyyMMdd}.txt");

                var sb = new StringBuilder();
                sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] POST {requestUri}");
                sb.AppendLine($"  REQUEST : {requestJson}");
                sb.AppendLine($"  STATUS  : {statusCode}");
                sb.AppendLine($"  RESPONSE: {responseBody}");
                sb.AppendLine(new string('-', 80));

                File.AppendAllText(filePath, sb.ToString());
            }
            catch
            {
              
            }
        }
    }
}
