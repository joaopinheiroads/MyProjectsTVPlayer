using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TVPlayerSite.Disparos;
using Xunit;

namespace TVPlayerSite.Tests
{
    // Testes de unidade do DisparosDigisac. Cobrem o que descobrimos "na marra" depurando o
    // template verify3: a MONTAGEM do payload (componentes body/botão e o índice do botão) e a
    // CONFIRMAÇÃO de entrega assíncrona da Meta (o fix do falso SUCESSO — 201 do Digisac não é
    // "entregue"). Um HttpMessageHandler falso captura o POST e simula o status, sem tocar a rede.
    public class DisparosDigisacTests
    {
        private const string BaseUrl = "https://fake.digisac.test/api/v1/";
        private const string Token = "tok";
        private const string ServiceId = "svc-1";

        // Handler HTTP falso: guarda o corpo do POST e responde POST e GET (status) sob controle.
        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _postStatus;
            private readonly string _postBody;
            private readonly Func<int, (HttpStatusCode, string)> _getResponder;

            public string CapturedPostBody { get; private set; }
            public int PostCount { get; private set; }
            public int GetCount { get; private set; }

            public FakeHandler(HttpStatusCode postStatus, string postBody, Func<int, (HttpStatusCode, string)> getResponder = null)
            {
                _postStatus = postStatus;
                _postBody = postBody;
                _getResponder = getResponder;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.Method == HttpMethod.Post)
                {
                    PostCount++;
                    CapturedPostBody = request.Content == null ? null : await request.Content.ReadAsStringAsync();
                    return new HttpResponseMessage(_postStatus) { Content = new StringContent(_postBody ?? "") };
                }

                GetCount++;
                var resp = _getResponder != null ? _getResponder(GetCount) : (HttpStatusCode.OK, "{}");
                return new HttpResponseMessage(resp.Item1) { Content = new StringContent(resp.Item2 ?? "") };
            }
        }

        // POST 201 com id + ack 0 (pendente); GET devolve o ack informado (default: entregue "2").
        private static FakeHandler HandlerEntrega(string ackGet = "2")
            => new FakeHandler(
                HttpStatusCode.Created,
                "{\"id\":\"msg-1\",\"data\":{\"ack\":0}}",
                _ => (HttpStatusCode.OK, "{\"data\":{\"ack\":\"" + ackGet + "\"}}"));

        // Poll acelerado (1ms x 3) pra os testes não esperarem os 3,6s de produção.
        private static DisparosDigisac Criar(FakeHandler handler)
            => new DisparosDigisac(BaseUrl, Token, ServiceId, handler, pollDelayMs: 1, pollTentativas: 3);

        private static JToken Componente(JArray componentes, string tipo)
        {
            foreach (var c in componentes)
                if ((string)c["type"] == tipo)
                    return c;
            return null;
        }

        // ---------- Guardas de entrada (retornam antes de qualquer HTTP) ----------

        [Fact]
        public async Task EnviarTemplate_SemNumero_RetornaErroSemChamarHttp()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            var r = await sut.EnviarTemplateAsync("", "hsm-1", false, "x");

            Assert.False(r.Sucesso);
            Assert.Contains("telefone", r.Erro);
            Assert.Equal(0, handler.PostCount);
        }

        [Fact]
        public async Task EnviarTemplate_SemTemplateId_RetornaErro()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            var r = await sut.EnviarTemplateAsync("<REDACTED>", "", false, "x");

            Assert.False(r.Sucesso);
            Assert.Contains("templateId", r.Erro);
            Assert.Equal(0, handler.PostCount);
        }

        [Fact]
        public async Task EnviarTemplateComBotaoUrl_SemUrlParam_RetornaErro()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            var r = await sut.EnviarTemplateComBotaoUrlAsync("<REDACTED>", "hsm-1", "", 0, "nome");

            Assert.False(r.Sucesso);
            Assert.Equal(0, handler.PostCount);
        }

        [Fact]
        public async Task EnviarMensagem_SemNumero_RetornaErro()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            var r = await sut.EnviarMensagemAsync("", "oi");

            Assert.False(r.Sucesso);
            Assert.Equal(0, handler.PostCount);
        }

        // ---------- Montagem do payload ----------

        [Fact]
        public async Task EnviarTemplateComBotaoUrl_MontaBodyEBotaoUrlNoIndiceInformado()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            await sut.EnviarTemplateComBotaoUrlAsync("<REDACTED>", "hsm-1", "?p=abc&e=def", 0, "Joao", "TVPlayer");

            var payload = JObject.Parse(handler.CapturedPostBody);
            Assert.Equal("hsm-1", (string)payload["hsmId"]);
            var comps = (JArray)payload["parameters"];

            var body = Componente(comps, "body");
            Assert.NotNull(body);
            var bodyParams = (JArray)body["parameters"];
            Assert.Equal(2, bodyParams.Count);
            Assert.Equal("Joao", (string)bodyParams[0]["text"]);
            Assert.Equal("TVPlayer", (string)bodyParams[1]["text"]);

            var botao = Componente(comps, "button");
            Assert.NotNull(botao);
            Assert.Equal("url", (string)botao["sub_type"]);
            Assert.Equal(0, (int)botao["index"]);
            Assert.Equal("?p=abc&e=def", (string)botao["parameters"][0]["text"]);
        }

        [Fact]
        public async Task EnviarTemplateComBotaoUrl_RespeitaIndiceDiferenteDeZero()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            await sut.EnviarTemplateComBotaoUrlAsync("<REDACTED>", "hsm-1", "?p=abc", 1, "Joao");

            var comps = (JArray)JObject.Parse(handler.CapturedPostBody)["parameters"];
            Assert.Equal(1, (int)Componente(comps, "button")["index"]);
        }

        [Fact]
        public async Task EnviarTemplate_ComCopyCode_IncluiBotaoOtpComOCodigo()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            await sut.EnviarTemplateAsync("<REDACTED>", "hsm-otp", true, "1234");

            var comps = (JArray)JObject.Parse(handler.CapturedPostBody)["parameters"];

            var body = Componente(comps, "body");
            Assert.Single((JArray)body["parameters"]);
            Assert.Equal("1234", (string)body["parameters"][0]["text"]);

            var botao = Componente(comps, "button");
            Assert.NotNull(botao);
            Assert.Equal("url", (string)botao["sub_type"]);
            Assert.Equal(0, (int)botao["index"]);
            Assert.Equal("1234", (string)botao["parameters"][0]["text"]);
        }

        [Fact]
        public async Task EnviarTemplate_SemCopyCode_NaoIncluiBotao()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            await sut.EnviarTemplateAsync("<REDACTED>", "hsm-1", false, "abc");

            var comps = (JArray)JObject.Parse(handler.CapturedPostBody)["parameters"];
            Assert.Null(Componente(comps, "button"));
        }

        [Fact]
        public async Task EnviarTemplate_SemParametros_NaoIncluiComponenteBody()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            await sut.EnviarTemplateAsync("<REDACTED>", "hsm-sem-var", false);

            var comps = (JArray)JObject.Parse(handler.CapturedPostBody)["parameters"];
            Assert.Null(Componente(comps, "body"));
        }

        [Fact]
        public async Task EnviarTemplateComImagem_SemUrlImagem_RetornaErroSemChamarHttp()
        {
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            var r = await sut.EnviarTemplateComImagemAsync("<REDACTED>", "hsm-img", "", "Joao");

            Assert.False(r.Sucesso);
            Assert.Contains("urlImagem", r.Erro);
            Assert.Equal(0, handler.PostCount);
        }

        [Fact]
        public async Task EnviarTemplateComImagem_MontaHeaderComImagemEBody()
        {
            const string urlImagem = "https://www.tvplayer.com.br/img/cabecalho.png";
            var handler = HandlerEntrega();
            var sut = Criar(handler);

            await sut.EnviarTemplateComImagemAsync("<REDACTED>", "hsm-img", urlImagem, "Joao");

            var comps = (JArray)JObject.Parse(handler.CapturedPostBody)["parameters"];

            var header = Componente(comps, "header");
            Assert.NotNull(header);
            var imagem = header["parameters"][0];
            Assert.Equal("image", (string)imagem["type"]);
            Assert.Equal(urlImagem, (string)imagem["image"]["link"]);

            var body = Componente(comps, "body");
            Assert.Single((JArray)body["parameters"]);
            Assert.Equal("Joao", (string)body["parameters"][0]["text"]);

            Assert.Null(Componente(comps, "button"));
        }

        // ---------- Confirmação de entrega assíncrona (fix do falso SUCESSO) ----------

        [Fact]
        public async Task Envio_QuandoMetaRejeita_RetornaFalhaComMotivoReal()
        {
            // Digisac aceita (201/ack 0), mas o webhook da Meta marca ack "error" com o motivo.
            var erroBody =
                "{\"data\":{\"ack\":\"error\",\"error\":{" +
                "\"message\":\"(#131008) Required parameter is missing\"," +
                "\"error_data\":{\"details\":\"buttons: Button at index 1 of type Url requires a parameter\"}}}}";
            var handler = new FakeHandler(
                HttpStatusCode.Created,
                "{\"id\":\"msg-1\",\"data\":{\"ack\":0}}",
                _ => (HttpStatusCode.OK, erroBody));
            var sut = Criar(handler);

            var r = await sut.EnviarTemplateComBotaoUrlAsync("<REDACTED>", "hsm-1", "?p=abc", 1, "Joao", "Emp");

            Assert.False(r.Sucesso);
            Assert.Contains("131008", r.Erro);
            Assert.Contains("index 1", r.Erro);
        }

        [Fact]
        public async Task Envio_QuandoEntregue_RetornaSucesso()
        {
            var handler = HandlerEntrega(ackGet: "2");
            var sut = Criar(handler);

            var r = await sut.EnviarTemplateAsync("<REDACTED>", "hsm-1", false, "x");

            Assert.True(r.Sucesso);
            Assert.Null(r.Erro);
        }

        [Fact]
        public async Task Envio_QuandoPendente_TrataComoAceitoAposTimeout()
        {
            // GET sempre "pendente" (ack 0): o poll estoura o limite (3 tentativas) e, sem erro
            // conhecido, trata como aceito (entrega pendente) — não vira falha.
            var handler = new FakeHandler(
                HttpStatusCode.Created,
                "{\"id\":\"msg-1\",\"data\":{\"ack\":0}}",
                _ => (HttpStatusCode.OK, "{\"data\":{\"ack\":\"0\"}}"));
            var sut = Criar(handler);

            var r = await sut.EnviarTemplateAsync("<REDACTED>", "hsm-1", false, "x");

            Assert.True(r.Sucesso);
            Assert.Equal(3, handler.GetCount);
        }

        [Fact]
        public async Task Envio_QuandoDigisacRetornaNao2xx_RetornaFalhaSemPoll()
        {
            var handler = new FakeHandler(HttpStatusCode.BadRequest, "erro digisac");
            var sut = Criar(handler);

            var r = await sut.EnviarTemplateAsync("<REDACTED>", "hsm-1", false, "x");

            Assert.False(r.Sucesso);
            Assert.Contains("400", r.Erro);
            Assert.Equal(0, handler.GetCount); // POST falhou -> não consulta status
        }
    }
}
