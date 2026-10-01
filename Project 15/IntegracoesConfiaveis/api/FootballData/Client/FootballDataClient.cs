using SerializableObjects.PluginsTVPlayer;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using TVPlayerAPI.FootballData.ResponseWrappers;
using TVPlayerAPI.Helpers;
using TVPlayerAPI.Interfaces;

namespace TVPlayerAPI.FootballData.Client
{
    internal class FootballDataClient : IPlugin
    {
        private const string mainLink = "https://api.football-data.org";
        private const string FORMATO_DATA = "yyyy-MM-dd";

        private static readonly Logger logger = LogManager.GetLogger("TVPlayerAPI.FootballData.Client");

        private static readonly string ApiKey =
            ConfigurationManager.AppSettings["FootballDataApiKey"] ?? string.Empty;

        private static readonly HttpDedicado http =
            new HttpDedicado("FootballData", new Dictionary<string, string> { { "X-Auth-Token", ApiKey } });

        private string[] competitions = { "CL", "BSA" };
        private string savePath;

        public FootballDataClient(string savePath)
        {
            this.savePath = savePath;
            IOHelper.CheckDirectory(this.savePath);
        }

        private string ReturnCompetitionParams(CompetitionInfo competition)
        {
            switch (competition.Competition.Code)
            {
                case "BSA":
                    return ParametrosBSA();
                case "CL":
                    return ParametrosCL(competition);
                default:
                    return string.Empty;
            }
        }

        private string ParametrosBSA()
        {
            DateTime inicioTemporada = LerLimite("Football_BSA_InicioTemporadaAte");
            DateTime fimTemporada = LerLimite("Football_BSA_FimTemporadaAte");

            if (DateTime.Now < inicioTemporada)
                return $"?dateFrom={LerLimite("Football_BSA_PreTemporadaDe"):yyyy-MM-dd}&dateTo={inicioTemporada:yyyy-MM-dd}";

            if (DateTime.Now < fimTemporada)
                return $"?dateFrom={DateTime.UtcNow.AddDays(-3):yyyy-MM-dd}&dateTo={DateTime.UtcNow.AddDays(4):yyyy-MM-dd}";

            return $"?matchday={ConfigurationManager.AppSettings["Football_BSA_UltimaRodada"]}";
        }

        private string ParametrosCL(CompetitionInfo competition)
        {
            DateTime faseLiga = LerLimite("Football_CL_FaseLigaAte");
            if (DateTime.Now <= faseLiga)
                return $"?matchday={competition.Season.CurrentMatchday}";

            DateTime playoff = LerLimite("Football_CL_PlayoffAte");
            if (DateTime.Now < playoff)
                return $"?matchday={ConfigurationManager.AppSettings["Football_CL_PlayoffRodada"]}";

            DateTime oitavasIda = LerLimite("Football_CL_OitavasIdaAte");
            if (DateTime.Now < oitavasIda)
                return Janela("LAST_16", playoff, oitavasIda);

            DateTime oitavasVolta = LerLimite("Football_CL_OitavasVoltaAte");
            if (DateTime.Now < oitavasVolta)
                return Janela("LAST_16", oitavasIda, oitavasVolta);

            DateTime quartasIda = LerLimite("Football_CL_QuartasIdaAte");
            if (DateTime.Now < quartasIda)
                return Janela("QUARTER_FINALS", oitavasVolta, quartasIda);

            DateTime quartasVolta = LerLimite("Football_CL_QuartasVoltaAte");
            if (DateTime.Now < quartasVolta)
                return Janela("QUARTER_FINALS", quartasIda, quartasVolta);

            DateTime semisIda = LerLimite("Football_CL_SemisIdaAte");
            if (DateTime.Now < semisIda)
                return Janela("SEMI_FINALS", quartasVolta, semisIda);

            DateTime semisVolta = LerLimite("Football_CL_SemisVoltaAte");
            if (DateTime.Now < semisVolta)
                return Janela("SEMI_FINALS", semisIda, semisVolta);

            return "?stage=FINAL";
        }

        private static string Janela(string stage, DateTime inicio, DateTime limite)
        {
            return $"?stage={stage}&dateFrom={inicio:yyyy-MM-dd}&dateTo={limite.AddDays(-1):yyyy-MM-dd}";
        }

        private static DateTime LerLimite(string chave)
        {
            string valor = ConfigurationManager.AppSettings[chave];

            if (DateTime.TryParseExact(valor, FORMATO_DATA, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime limite))
                return limite;

            logger.Warn("appSettings[" + chave + "] ausente ou fora do formato " + FORMATO_DATA + ": " + (valor ?? "<null>"));
            return DateTime.MinValue;
        }

        private async Task<T> RunSearchAsync<T>(string urlPath)
        {
            try { return await http.ObterEDesserializarAsync<T>(urlPath); }
            catch (Exception ex) { return default; }
        }
    }
}
