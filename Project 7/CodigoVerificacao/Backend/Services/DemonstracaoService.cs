using System;
using System.Threading.Tasks;
using TVPlayerSite.API.Interfaces.CRM;
using TVPlayerSite.API.Helpers;




namespace TVPlayerSite.API.Services
{
    public class DemonstracaoService : IDemonstracaoService
    {


        public static string ObterSaudacao()
        {
            var horaAtual = DateTime.Now.Hour;

            if (horaAtual >= 5 && horaAtual < 12)
                return "bom dia";
            else if (horaAtual >= 12 && horaAtual < 18)
                return "boa tarde";
            else
                return "boa noite";
        }

        string saudacao = ObterSaudacao();


        public async Task<bool> EnviarCodigoPorEmail(string email, string nome, string codigo)
        {

            bool sucesso = false;
            string erro = null;

            try
            {
                var emailHelper = new EmailHelper();
                var resultado = emailHelper.EnviarEmailCodigoVerificacaoComDetalhes(email, nome, codigo, saudacao);
                sucesso = resultado.sucesso;
                erro = resultado.erro;

                if (!sucesso && string.IsNullOrEmpty(erro))
                {
                    erro = "Falha no envio do email de código de verificação (erro desconhecido)";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
                sucesso = false;
                erro = ex.Message;
            }
            finally
            {
                // Log do disparo de email
                await LogVerificacaoHelper.LogDisparoEmail(email, "CODIGO_VERIFICACAO", sucesso, erro);
            }


            return sucesso;
        }


    } 

}
