using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.Services.Demonstracao
{
    // --- Refatoração passo 6: log de verificação injetável ---
    //
    // Antes, o log era um helper ESTÁTICO (LogVerificacaoHelper) chamado direto de dentro
    // do controller e dos serviços. Isso é uma dependência "escondida" (não aparece no
    // construtor) e não substituível: não dá para trocar por um log em banco/nuvem, nem
    // para silenciar/mockar em teste.
    //
    // Aqui o mesmo comportamento vira um serviço injetado por interface (DIP). Quem loga
    // recebe IVerificacaoLogger no construtor e passa a depender de uma abstração, não de
    // um método estático concreto. A lógica de arquivo é idêntica à do helper antigo.
    public interface IVerificacaoLogger
    {
        Task LogInicioProcesso(string nome, string telefone, string email);
        Task LogDisparoWhatsApp(string telefone, string codigo, bool sucesso, string erro = null);
        Task LogDisparoEmail(string email, string tipo, bool sucesso, string erro = null);
        Task LogTentativaVerificacao(string identificacao, string codigoInformado, bool sucesso);
        Task LogExpiracaoTempo(string clienteJson);
        Task LogSucessoCadastro(string nome, string telefone, string email);
    }

    public sealed class VerificacaoLogger : IVerificacaoLogger
    {
        // Pasta de logs do código verificador
        private static readonly string LogDirectory = GetLogDirectory();

        // Pasta de logs de email
        private static readonly string LogEmailDirectory = GetLogEmailDirectory();

        private static string GetLogDirectory()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "LogsCodigoVerificador");
        }

        private static string GetLogEmailDirectory()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "LogsEmail");
        }

        private async Task EscreverLog(string message)
        {
            try
            {
                var fullLogPath = Path.GetFullPath(LogDirectory);
                if (!Directory.Exists(fullLogPath))
                    Directory.CreateDirectory(fullLogPath);

                string fileName = $"CodigoVerificador_{DateTime.Now:yyyyMMdd}.txt";
                string filePath = Path.Combine(fullLogPath, fileName);

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";

                await File.AppendAllTextAsync(filePath, logMessage);

                LimparLogsAntigos(fullLogPath, "CodigoVerificador_*.txt");
            }
            catch (Exception ex)
            {
                // Fallback: se o próprio log falhar, ainda reportamos no console para não
                // interromper o fluxo (é o único canal restante para sinalizar a falha).
                Console.WriteLine($"[ERRO LOG VERIFICACAO] {ex.Message}");
            }
        }

        private async Task EscreverLogEmail(string message)
        {
            try
            {
                var fullLogPath = Path.GetFullPath(LogEmailDirectory);
                if (!Directory.Exists(fullLogPath))
                    Directory.CreateDirectory(fullLogPath);

                string fileName = $"Email_{DateTime.Now:yyyyMMdd}.txt";
                string filePath = Path.Combine(fullLogPath, fileName);

                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";

                await File.AppendAllTextAsync(filePath, logMessage);

                LimparLogsAntigos(fullLogPath, "Email_*.txt");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO LOG EMAIL] {ex.Message}");
            }
        }

        private void LimparLogsAntigos(string logPath, string filePattern)
        {
            try
            {
                var arquivos = Directory.GetFiles(logPath, filePattern);
                foreach (var arquivo in arquivos)
                {
                    var info = new FileInfo(arquivo);
                    if (info.CreationTime < DateTime.Now.AddDays(-30))
                    {
                        File.Delete(arquivo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO LIMPEZA LOG] {ex.Message}");
            }
        }

        public async Task LogInicioProcesso(string nome, string telefone, string email)
        {
            string message = $"INICIO_PROCESSO | Nome: {nome} | Telefone: {telefone} | Email: {email}";
            await EscreverLog(message);
        }

        public async Task LogDisparoWhatsApp(string telefone, string codigo, bool sucesso, string erro = null)
        {
            string status = sucesso ? "SUCESSO" : "FALHA";
            string message = $"WHATSAPP_{status} | Telefone: {telefone} | Codigo: {codigo}";

            if (!sucesso && !string.IsNullOrEmpty(erro))
            {
                message += $" | Erro: {erro}";
            }

            await EscreverLog(message);
        }

        public async Task LogDisparoEmail(string email, string tipo, bool sucesso, string erro = null)
        {
            string status = sucesso ? "SUCESSO" : "FALHA";
            string message = $"EMAIL_{status} | Email: {email} | Tipo: {tipo}";

            if (!sucesso && !string.IsNullOrEmpty(erro))
            {
                message += $" | Erro: {erro}";
            }

            await EscreverLogEmail(message);
        }

        public async Task LogTentativaVerificacao(string identificacao, string codigoInformado, bool sucesso)
        {
            string status = sucesso ? "CODIGO_CORRETO" : "CODIGO_INCORRETO";
            string message = $"VERIFICACAO_{status} | Usuario: {identificacao} | Codigo_Informado: {codigoInformado}";

            await EscreverLog(message);
        }

        public async Task LogExpiracaoTempo(string clienteJson)
        {
            try
            {
                var cliente = JsonConvert.DeserializeObject<TabCliente>(clienteJson);
                string message = $"CODIGO_EXPIRADO | Nome: {cliente.CliNome} | Telefone: {cliente.CliTelefoneCelular} | Email: {cliente.CliEmail}";
                await EscreverLog(message);
            }
            catch (Exception ex)
            {
                string message = $"CODIGO_EXPIRADO | Erro_ao_processar_dados: {ex.Message}";
                await EscreverLog(message);
            }
        }

        public async Task LogSucessoCadastro(string nome, string telefone, string email)
        {
            string message = $"CADASTRO_CONCLUIDO | Nome: {nome} | Telefone: {telefone} | Email: {email}";
            await EscreverLog(message);
        }
    }
}
