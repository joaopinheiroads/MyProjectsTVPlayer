using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.Helpers
{
    public static class LogVerificacaoHelper
    {
        // Caminho para a pasta de logs do código verificador
        private static readonly string LogDirectory = GetLogDirectory();
        
        // Caminho para a pasta de logs de email
        private static readonly string LogEmailDirectory = GetLogEmailDirectory();
        
        private static string GetLogDirectory()
        {
            // Usa a mesma estratégia do VerificadorDeDemonstracoesLogger
            // Cria a pasta LogsCodigoVerificador no mesmo diretório base da aplicação
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var logsPath = Path.Combine(baseDir, "LogsCodigoVerificador");
            return logsPath;
        }

        private static string GetLogEmailDirectory()
        {
            // Cria a pasta LogsEmail no mesmo diretório base da aplicação
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var logsPath = Path.Combine(baseDir, "LogsEmail");
            return logsPath;
        }
      
        /// <param name="message">Mensagem a ser registrada no log</param>
        private static async Task EscreverLog(string message)
        {
            try
            {
                // Garantir que o diretório existe
                var fullLogPath = Path.GetFullPath(LogDirectory);
                if (!Directory.Exists(fullLogPath))
                    Directory.CreateDirectory(fullLogPath);

                // Nome do arquivo baseado na data
                string fileName = $"CodigoVerificador_{DateTime.Now:yyyyMMdd}.txt";
                string filePath = Path.Combine(fullLogPath, fileName);
                
                // Formatar mensagem com timestamp
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                
                // Escrever no arquivo de forma assíncrona
                await File.AppendAllTextAsync(filePath, logMessage);
                
                // Limpar logs antigos (mais de 30 dias)
                LimparLogsAntigos(fullLogPath, "CodigoVerificador_*.txt");
            }
            catch (Exception ex)
            {
                // Em caso de erro, registrar no console para não interromper o fluxo
                Console.WriteLine($"[ERRO LOG VERIFICACAO] {ex.Message}");
            }
        }

        /// <summary>
        /// Escreve logs específicos de email na pasta LogsEmail
        /// </summary>
        /// <param name="message">Mensagem a ser registrada no log de email</param>
        private static async Task EscreverLogEmail(string message)
        {
            try
            {
                // Garantir que o diretório de email existe
                var fullLogPath = Path.GetFullPath(LogEmailDirectory);
                if (!Directory.Exists(fullLogPath))
                    Directory.CreateDirectory(fullLogPath);

                // Nome do arquivo baseado na data
                string fileName = $"Email_{DateTime.Now:yyyyMMdd}.txt";
                string filePath = Path.Combine(fullLogPath, fileName);
                
                // Formatar mensagem com timestamp
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                
                // Escrever no arquivo de forma assíncrona
                await File.AppendAllTextAsync(filePath, logMessage);
                
                // Limpar logs antigos (mais de 30 dias)
                LimparLogsAntigos(fullLogPath, "Email_*.txt");
            }
            catch (Exception ex)
            {
                // Em caso de erro, registrar no console para não interromper o fluxo
                Console.WriteLine($"[ERRO LOG EMAIL] {ex.Message}");
            }
        }
      
        /// </summary>
        /// <param name="logPath">Caminho da pasta de logs</param>
        /// <param name="filePattern">Padrão dos arquivos a serem limpos</param>
        private static void LimparLogsAntigos(string logPath, string filePattern)
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
    
        public static async Task LogInicioProcesso(string nome, string telefone, string email)
        {
            string message = $"INICIO_PROCESSO | Nome: {nome} | Telefone: {telefone} | Email: {email}";
            await EscreverLog(message);
        }
     
        public static async Task LogDisparoWhatsApp(string telefone, string codigo, bool sucesso, string erro = null)
        {
            string status = sucesso ? "SUCESSO" : "FALHA";
            string message = $"WHATSAPP_{status} | Telefone: {telefone} | Codigo: {codigo}";
            
            if (!sucesso && !string.IsNullOrEmpty(erro))
            {
                message += $" | Erro: {erro}";
            }
            
            await EscreverLog(message);
        }
      
        public static async Task LogDisparoEmail(string email, string tipo, bool sucesso, string erro = null)
        {
            string status = sucesso ? "SUCESSO" : "FALHA";
            string message = $"EMAIL_{status} | Email: {email} | Tipo: {tipo}";
            
            if (!sucesso && !string.IsNullOrEmpty(erro))
            {
                message += $" | Erro: {erro}";
            }
            
            // Usar EscreverLogEmail para salvar na pasta LogsEmail
            await EscreverLogEmail(message);
        }
        
     
        public static async Task LogTentativaVerificacao(string identificacao, string codigoInformado, bool sucesso)
        {
            string status = sucesso ? "CODIGO_CORRETO" : "CODIGO_INCORRETO";
            string message = $"VERIFICACAO_{status} | Usuario: {identificacao} | Codigo_Informado: {codigoInformado}";
            
            await EscreverLog(message);
        }
       
        public static async Task LogExpiracaoTempo(string clienteJson)
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
        
        
                public static async Task LogSucessoCadastro(string nome, string telefone, string email)
        {
            string message = $"CADASTRO_CONCLUIDO | Nome: {nome} | Telefone: {telefone} | Email: {email}";
            await EscreverLog(message);
        }
    }
}