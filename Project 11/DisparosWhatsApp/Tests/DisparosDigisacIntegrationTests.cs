using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TVPlayerSite.Disparos;
using TVPlayerSite.Services.Demonstracao;
using Xunit;

namespace TVPlayerSite.Tests
{
    // ---------------------------------------------------------------------------------------------
    // Testes de INTEGRAÇÃO do serviço de template: disparam DE VERDADE contra a API do Digisac e
    // usam o poll de confirmação para checar o status real da Meta (ack >= 1 = entregue, ack "error"
    // = a Meta descartou).
    //
    // Ficam SEPARADOS dos unitários: sem o interruptor abaixo, todos são Skipped, e o `dotnet test`
    // do dia a dia não sai disparando WhatsApp. Pra rodar:
    //   $env:DIGISAC_INTEGRATION = "1"      (vale só na janela atual do PowerShell)
    //   dotnet test --filter "Category=Integration"
    // Só os unitários: dotnet test --filter "Category!=Integration"
    //
    // Quando rodam, rodam TODOS os templates dos dois projetos — um teste por template.
    //
    // O destino é SEMPRE NumeroDono (o número do dono do projeto), nunca o número de um cliente.
    // Nenhum teste aqui lê telefone de config/banco, justamente pra não existir caminho que dispare
    // para usuário real.
    //
    // A config (baseUrl, token, serviceId, templateIds) vem dos MESMOS appsettings.json de produção,
    // copiados pro output do teste pelo .csproj:
    //   appsettings.json      -> TVPlayerSite      (templates do site)
    //   appsettings.api.json  -> TVPlayerSite.API  (templates demo e disparodemo)
    // Ler os dois separadamente é de propósito: se o appsettings da API divergir do site, o teste
    // acusa, porque cada disparo usa o id do projeto que realmente o envia em produção.
    //
    // Cada teste espelha a chamada que a produção faz (mesmos parâmetros, mesmo urlButtonIndex).
    // Se o template mudar no painel do Digisac (variável a mais, botão reordenado), o teste quebra.
    // ---------------------------------------------------------------------------------------------
    [Trait("Category", "Integration")]
    public class DisparosDigisacIntegrationTests
    {
        // Número do dono do projeto. Constante de propósito: destino fixo, nunca vem de config.
        private const string NumeroDono = "<REDACTED>";

        private const string NomeCliente = "Teste Integração";
        private const string NomeEmpresa = "TVPlayer";

        private static readonly JObject DigisacSite = CarregarDigisac("appsettings.json");
        private static readonly JObject DigisacApi = CarregarDigisac("appsettings.api.json");

        private static readonly DadosService Dados = new DadosService();

        // Formata igual à produção (DDI 55 + capitalização), a partir da constante acima.
        private static string Numero => Dados.NormalizarBrasil(NumeroDono);
        private static string Nome => Dados.FormatarNomePessoa(NomeCliente);
        private static string Empresa => Dados.FormatarNomeEmpresa(NomeEmpresa);

        private static JObject CarregarDigisac(string arquivo)
        {
            var path = Path.Combine(AppContext.BaseDirectory, arquivo);
            if (!File.Exists(path)) return null;
            return (JObject)JObject.Parse(File.ReadAllText(path))["Digisac"];
        }

        // Interruptor de segurança + pré-requisitos. Se algo faltar, PULA (Skip) em vez de falhar.
        private static void GarantirHabilitado()
        {
            Skip.IfNot(Environment.GetEnvironmentVariable("DIGISAC_INTEGRATION") == "1",
                "Defina DIGISAC_INTEGRATION=1 para rodar (dispara WhatsApp real).");
            Skip.If(DigisacSite == null,
                "appsettings.json não encontrado no output do teste (verifique o <None Include> no .csproj).");
            Skip.If(DigisacApi == null,
                "appsettings.api.json não encontrado no output do teste (verifique o <None Include> no .csproj).");
        }

        // Falha explícita em vez de mandar hsmId vazio pro Digisac (que responderia com erro opaco).
        private static string Cfg(JObject digisac, string chave)
        {
            var valor = (string)digisac[chave];
            Assert.False(string.IsNullOrWhiteSpace(valor), $"Digisac:{chave} não está preenchido no appsettings.");
            return valor;
        }

        private static string CfgSite(string chave) => Cfg(DigisacSite, chave);
        private static string CfgApi(string chave) => Cfg(DigisacApi, chave);

        private static DisparosDigisac CriarSite()
            => new DisparosDigisac(CfgSite("BaseUrl"), CfgSite("Token"), CfgSite("ServiceId"));

        private static DisparosDigisac CriarApi()
            => new DisparosDigisac(CfgApi("BaseUrl"), CfgApi("Token"), CfgApi("ServiceId"));

        // -------------------------------------------------------------------------------------
        // Templates do SITE
        // -------------------------------------------------------------------------------------

        // Espelha CadastroClienteService.ProcessarClienteNovoAsync.
        [SkippableFact]
        public async Task ConfirmEmail_DisparaEEntregaDeVerdade()
        {
            GarantirHabilitado();
            var sut = CriarSite();

            var r = await sut.EnviarTemplateComBotaoUrlAsync(
                Numero, CfgSite("ConfirmEmailTemplateId"), "?p=teste-integracao&e=abc", 0, Nome, Empresa);

            Assert.True(r.Sucesso, r.Erro);
        }

        // Espelha CadastroClienteService.ProcessarClienteExistenteAsync.
        [SkippableFact]
        public async Task UserExist_DisparaEEntregaDeVerdade()
        {
            GarantirHabilitado();
            var sut = CriarSite();

            var r = await sut.EnviarTemplateComBotaoUrlAsync(
                Numero, CfgSite("UserExistTemplateId"), "?p=teste-integracao&e=abc", 0, Nome, Empresa);

            Assert.True(r.Sucesso, r.Erro);
        }

        // Espelha DemonstracaoAppService — código OTP no botão de copiar.
        [SkippableFact]
        public async Task Otp_DisparaEEntregaDeVerdade()
        {
            GarantirHabilitado();
            var sut = CriarSite();

            var r = await sut.EnviarTemplateAsync(Numero, CfgSite("CodeTemplateId"), comBotaoCopyCode: true, "9999");

            Assert.True(r.Sucesso, r.Erro);
        }


        [SkippableFact]
        public async Task Otp_DisparoTesteAccountSuccess_9()
        {
            GarantirHabilitado();
            var sut = CriarSite();

            var r = await sut.EnviarTemplateComImagemAsync(Numero, CfgSite("AccountSuccessTemplateId2"), CfgSite("AccountSuccessImagemUrl"), "João");

            Assert.True(r.Sucesso, r.Erro);
        }

        // Espelha DemonstracaoAppService — aviso de conta criada.
        [SkippableFact]
        public async Task EmailSucesso_DisparaEEntregaDeVerdade()
        {
            GarantirHabilitado();
            var sut = CriarSite();

            var r = await sut.EnviarTemplateAsync(Numero, CfgSite("AccountSuccessTemplateId"), comBotaoCopyCode: false, Nome, Empresa);

            Assert.True(r.Sucesso, r.Erro);
        }

        // -------------------------------------------------------------------------------------
        // Templates da API
        // -------------------------------------------------------------------------------------

        // Template "disparodemo": espelha DisparoDemonstracao.EnviarWhatsAppDemonstracaoAsync —
        // botão de URL recebe o link de confirmação e o body leva só o e-mail do cliente.
        [SkippableFact]
        public async Task DisparoDemo_DisparaEEntregaDeVerdade()
        {
            GarantirHabilitado();
            var sut = CriarApi();

            var r = await sut.EnviarTemplateComBotaoUrlAsync(
                Numero, CfgApi("DisparoDemoTemplateId"), "?p=teste-integracao&e=abc", 0, "<REDACTED>");

            Assert.True(r.Sucesso, r.Erro);
        }

        // Template "demo" (penúltimo dia): espelha VerificadorDeDemonstracoes — 2 quick-replies,
        // sem botão de URL.
        [SkippableFact]
        public async Task DemoPenultimoDia_DisparaEEntregaDeVerdade()
        {
            GarantirHabilitado();
            var sut = CriarApi();

            var r = await sut.EnviarTemplateComBotaoUrlAsync(Numero, CfgApi("DemoPenultimoTemplateId"), "<REDACTED>", 0, Nome, Empresa);

            Assert.True(r.Sucesso, r.Erro);
        }
    }
}
