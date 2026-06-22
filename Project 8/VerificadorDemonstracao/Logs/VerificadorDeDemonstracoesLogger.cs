using System;
using System.IO;
using System.Linq;

namespace TVPlayerSite.API.Logs
{
    public static class VerificadorDeDemonstracoesLogger
    {
        // Caminho absoluto para a pasta Logs na raiz do projeto (TVPlayerSite.API/Logs)
        private static readonly string logDirectory = GetLogDirectory();

        private static string GetLogDirectory()
        {
            // Caminho fixo para a pasta Logs na raiz do projeto
            // Considera que a aplicação está rodando a partir da pasta bin/{config}/netcoreapp2.2
            // Sobe três níveis para chegar na raiz do projeto e adiciona "Logs"
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
       
            //var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            var logsPath = Path.Combine(baseDir, "Logs");
            return logsPath;
        }

        public static void Log(string message)
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                // Apaga logs com mais de 30 dias
                var files = Directory.GetFiles(logDirectory, "VerificadorDeDemonstracoes_*.log");
                foreach (var file in files)
                {
                    var creation = File.GetCreationTime(file);
                    if ((DateTime.Now - creation).TotalDays > 30)
                        File.Delete(file);
                }

                string fileName = $"VerificadorDeDemonstracoes_{DateTime.Now:yyyyMMdd}.log";
                string filePath = Path.Combine(logDirectory, fileName);
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";

                using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGGING ERROR] {ex.Message}");
            }
        }
    }
}
