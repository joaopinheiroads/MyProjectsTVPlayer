using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using TVPlayer.CRUD.Models;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.API.Helpers
{
    public class EmailHelper
    {
        private static readonly MailAddress siteMailAddress = new MailAddress("site@tvplayer.com.br", "Mensagem Automática - TV Player");
        private static readonly MailAddress contatoMailAddress = new MailAddress("contato@tvplayer.com.br", "Contato - TV Player");
        private static readonly MailAddress suporteMailAddress = new MailAddress("suporte@tvplayer.com.br", "Suporte - TV Player");


        private string GetEmailBody(string filePath)
        {
            WebClient webClient = new WebClient();
            byte[] pageHTMLBytes = webClient.DownloadData(filePath);
            UTF8Encoding _UTF8 = new UTF8Encoding();
            return _UTF8.GetString(pageHTMLBytes);
        }

        private static SmtpClient SmtpTVPlayer => new SmtpClient("smtp.tvplayer.com.br", 587)
        {
            Credentials = new NetworkCredential("site@tvplayer.com.br", "[SENHA_SMTP_REMOVIDA]"), // mover para configuração/variável de ambiente
            Timeout = 30000
        };

        private static MailMessage MailTVPlayer(List<MailAddress> mailReplyToList, List<MailAddress> mailToList, List<MailAddress> mailBccList, string subject, string body, bool isBodyHtml = false)
        {
            MailMessage mailMessage = new MailMessage()
            {
                From = siteMailAddress,
                Sender = siteMailAddress,
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml,
                BodyEncoding = Encoding.UTF8
            };

            foreach (MailAddress mailAddress in mailReplyToList)
            {
                mailMessage.ReplyToList.Add(mailAddress);
            }
            foreach (MailAddress mailAddress in mailToList)
            {
                mailMessage.To.Add(mailAddress);
            }
            foreach (MailAddress mailAddress in mailBccList)
            {
                mailMessage.Bcc.Add(mailAddress);
            }
            return mailMessage;
        }

        public bool EnviarEmailCompra(TabCliente compra)
        {
            string hardware = "Hardware";
            string software = "Software - TV Player";
            string titulo = string.Empty;
            if (compra.ComprarHardware == true && compra.ComprarSoftware == true)
                titulo = $"{hardware} e {software}";
            else if (compra.ComprarHardware == true)
                titulo = hardware;
            else if (compra.ComprarSoftware == true)
                titulo = software;

            var userMailAddress = new MailAddress(compra.CliEmail, compra.CliNome);
            var emailSubject = "[TV Player] Interesse de Compra - Contato via Site";
            var emailBody =
                $"Nome:     {compra.CliNome}{Environment.NewLine}" +
                $"Empresa:     {compra.CliEmpresa}{Environment.NewLine}" +
                $"CEP:   {compra.CEP}{Environment.NewLine}" +
                $"UF:   {compra.CliEstadoId}{Environment.NewLine}" +
                $"Cidade:   {compra.CidadeNome}{Environment.NewLine}" +
                $"E-mail:   {compra.CliEmail}{Environment.NewLine}" +
                $"Telefone:   {compra.CliTelefoneFixo}{Environment.NewLine}" +
                $"Celular:   {compra.CliTelefoneCelular}{Environment.NewLine}" +
                $"Email:   {compra.CliEmail}{Environment.NewLine}" +
                $"----------{Environment.NewLine}" +
                $"Desejo comprar:     {titulo}{Environment.NewLine}";

            if (!string.IsNullOrEmpty(compra.CliObservacao))
            {
                emailBody += $"----------{Environment.NewLine}Mensagem:   {compra.CliObservacao}{Environment.NewLine}";
            }

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress },
                    mailToList: new List<MailAddress> { contatoMailAddress },
                    mailBccList: new List<MailAddress> { },
                    subject: emailSubject,
                    body: emailBody));
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool EnviarEmailContato(TabCliente contato)
        {
            var userMailAddress = new MailAddress(contato.CliEmail, contato.CliNome);
            var emailSubject = "[TV Player] Contato via Site";
            var emailBody =
                $"Nome:     {contato.CliNome}{Environment.NewLine}" +
                $"Empresa:     {contato.CliEmpresa}{Environment.NewLine}" +
                $"CEP:   {contato.CEP}{Environment.NewLine}" +
                $"UF:   {contato.CliEstadoId}{Environment.NewLine}" +
                $"Cidade:   {contato.CidadeNome}{Environment.NewLine}" +
                $"E-mail:   {contato.CliEmail}{Environment.NewLine}" +
                $"Telefone:   {contato.CliTelefoneFixo}{Environment.NewLine}" +
                $"Celular:   {contato.CliTelefoneCelular}{Environment.NewLine}" +
                $"Email:   {contato.CliEmail}{Environment.NewLine}";
            if (!string.IsNullOrEmpty(contato.CliObservacao))
            {
                emailBody += $"----------{Environment.NewLine}Mensagem:   {contato.CliObservacao}{Environment.NewLine}";
            }

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress },
                    mailToList: new List<MailAddress> { contatoMailAddress },
                    mailBccList: new List<MailAddress> { },
                    subject: emailSubject,
                    body: emailBody));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnviarEmailDemonstracaoExistente(string path, TVPlayerSite.Models.CRM.Usuario usuario)
        {
            var link = $"https://app.tvplayer.com.br:44909/digitalsignage/sadm/pages/solicitacaocontratacao/frmcontratacao.aspx?tvp={EncryptHelper.Encrypt(usuario.ID.ToString())}";

            var userMailAddress = new MailAddress(usuario.Email, usuario.Nome);
            var emailSubject = "[TV Player] Demonstração - Email já cadastrado";
            var emailBody = GetEmailBody(path);
            emailBody = emailBody.Replace("[nome-cliente]", usuario.Nome.ToUpper());
            emailBody = emailBody.Replace("[link-contratacao]", link);

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress, contatoMailAddress },
                    mailToList: new List<MailAddress> { userMailAddress },
                    mailBccList: new List<MailAddress> { contatoMailAddress, suporteMailAddress },
                    subject: emailSubject,
                    body: emailBody,
                    isBodyHtml: true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnviarEmailSolicitacaoDemonstracao(string path, TabCliente cliente)
        {
            var licenca = new TabLicenca();
            foreach (var lic in cliente.TabLicenca)
            {
                licenca = lic;
                break; // Pegar apenas a primeira licença
            }
            
            // NÃO gerar nova chave aqui - usar a que já foi definida no controller
            // A chave deve vir sempre preenchida do controller
            if (string.IsNullOrEmpty(licenca.LicChave))
            {
                throw new InvalidOperationException("LicChave não pode estar vazia ao enviar email de confirmação");
            }
            
            var link = $"https://www.tvplayer.com.br/pt/demonstracao/validaremail?p={licenca.LicChave}&e={HexadecimalEncoding.ToHexString(cliente.CliEmail)}";

            var userMailAddress = new MailAddress(cliente.CliEmail, cliente.CliNome);
            var emailSubject = "[TV Player] Confirme seu e-mail para usar o software TV Player";
            var emailBody = GetEmailBody(path);
            emailBody = emailBody.Replace("[nome-cliente]", cliente.CliNome.ToUpper());
            emailBody = emailBody.Replace("[link-validate-email]", link);

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress, contatoMailAddress },
                    mailToList: new List<MailAddress> { userMailAddress },
                    mailBccList: new List<MailAddress> { contatoMailAddress, suporteMailAddress },
                    subject: emailSubject,
                    body: emailBody,
                    isBodyHtml: true));
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool EnviarEmailProcedimentoInstalacao(string path, UsuarioInfo usuario)
        {
            var userMailAddress = new MailAddress(usuario.UsuarioLogin, usuario.UsuarioNome);
            var emailSubject = "[TV Player] Procedimentos para demonstração";
            var emailBody = GetEmailBody(path);
            emailBody = emailBody.Replace("[nome-cliente]", usuario.UsuarioNome.ToUpper());
            emailBody = emailBody.Replace("[login-cliente]", usuario.UsuarioLogin);
            emailBody = emailBody.Replace("[senha-cliente]", usuario.UsuarioSenha);
            emailBody = emailBody.Replace("[chave-ativacao]", usuario.TerminalCodInventario);

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress, contatoMailAddress },
                    mailToList: new List<MailAddress> { userMailAddress },
                    mailBccList: new List<MailAddress> { contatoMailAddress, suporteMailAddress },
                    subject: emailSubject,
                    body: emailBody,
                    isBodyHtml: true));
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public bool EnviarEmailLigamosParaVoce(TabCliente contato)
        {
            var userMailAddress = new MailAddress(contato.CliEmail, contato.CliNome);
            var emailSubject = "[TV Player] Ligamos para você - Contato via Site";
            var emailBody =
                $"Nome:     {contato.CliNome}{Environment.NewLine}" +
                $"Telefone:   {contato.CliTelefoneFixo}{Environment.NewLine}" +
                $"Celular:   {contato.CliTelefoneCelular}{Environment.NewLine}" +
                $"Email:   {contato.CliEmail}{Environment.NewLine}";

            if (!string.IsNullOrEmpty(contato.CliObservacao))
            {
                emailBody += $"----------{Environment.NewLine}Mensagem:   {contato.CliObservacao}{Environment.NewLine}";
            }

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress },
                    mailToList: new List<MailAddress> { contatoMailAddress },
                    mailBccList: new List<MailAddress> { },
                    subject: emailSubject,
                    body: emailBody));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnviarEmailLigamosParaVoceHome(TabCliente contato)
        {
            var userMailAddress = new MailAddress(contato.CliEmail, contato.CliNome);
            var emailSubject = "[TV Player] Ligamos para você - Contato via Site";
            var emailBody =
                $"Nome:     {contato.CliNome}{Environment.NewLine}" +
                $"Empresa:     {contato.CliEmpresa}{Environment.NewLine}" +
                $"Celular:   {contato.CliTelefoneCelular}{Environment.NewLine}" +
                $"Email:   {contato.CliEmail}{Environment.NewLine}";

            if (!string.IsNullOrEmpty(contato.CliObservacao))
            {
                emailBody += $"----------{Environment.NewLine}Mensagem:   {contato.CliObservacao}{Environment.NewLine}";
            }

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress },
                    mailToList: new List<MailAddress> { contatoMailAddress },
                    mailBccList: new List<MailAddress> { },
                    subject: emailSubject,
                    body: emailBody));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnviarEmailNewsletter(Newsletter newsletter)
        {
            var userMailAddress = new MailAddress(newsletter.Email);
            var emailSubject = "[TV Player] Newsletter - Contato via Site";
            var emailBody = $"Foi efetuado o registro do e-mail: {newsletter.Email} para receber o Newsletter da TV Player";

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress },
                    mailToList: new List<MailAddress> { contatoMailAddress },
                    mailBccList: new List<MailAddress> { },
                    subject: emailSubject,
                    body: emailBody));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnviarEmailVideoconfAgendamento(VideoconfAgendamentoSite agendamento)
        {
            var userMailAddress = new MailAddress(agendamento.Email, agendamento.Nome);
            var emailSubject = "[TV Player] Agendamento de conferência via Site";
            var emailBody =
                $"Nome:     {agendamento.Nome}{Environment.NewLine}" +
                $"Empresa:     {agendamento.Empresa}{Environment.NewLine}" +
                $"Telefone:   {agendamento.Telefone}{Environment.NewLine}" +
                $"Email:   {agendamento.Email}{Environment.NewLine}" +
                "----------" + Environment.NewLine +
                $"Data:   {agendamento.Data.Value.ToShortDateString()}{Environment.NewLine}" +
                $"Horário:   {agendamento.Hora}:00 às {agendamento.Hora + 1}:00{Environment.NewLine}";

            if (!string.IsNullOrEmpty(agendamento.Observacoes))
            {
                emailBody += $"----------{Environment.NewLine}Mensagem:   {agendamento.Observacoes}{Environment.NewLine}";
            }

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress },
                    mailToList: new List<MailAddress> { contatoMailAddress },
                    mailBccList: new List<MailAddress> { },
                    subject: emailSubject,
                    body: emailBody));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnviarEmailRevenda(TabCliente contato)
        {
            var userMailAddress = new MailAddress(contato.CliEmail, contato.CliNome);
            var emailSubject = "[TV Player] Revenda - Contato via Site";
            var emailBody =
                $"Tipo:     REVENDA{Environment.NewLine}" +
                $"Nome:     {contato.CliNome}{Environment.NewLine}" +
                $"Empresa:     {contato.CliEmpresa}{Environment.NewLine}" +
                $"CEP:   {contato.CEP}{Environment.NewLine}" +
                $"UF:   {contato.CliEstadoId}{Environment.NewLine}" +
                $"Cidade:   {contato.CidadeNome}{Environment.NewLine}" +
                $"E-mail:   {contato.CliEmail}{Environment.NewLine}" +
                $"Telefone:   {contato.CliTelefoneFixo}{Environment.NewLine}" +
                $"Celular:   {contato.CliTelefoneCelular}{Environment.NewLine}" +
                $"Email:   {contato.CliEmail}{Environment.NewLine}";

            if (!string.IsNullOrEmpty(contato.CliObservacao))
            {
                emailBody += $"----------{Environment.NewLine}Mensagem:   {contato.CliObservacao}{Environment.NewLine}";
            }

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress, contatoMailAddress },
                    mailToList: new List<MailAddress> { userMailAddress },
                    mailBccList: new List<MailAddress> { contatoMailAddress },
                    subject: emailSubject,
                    body: emailBody));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool EnviarEmailCodigoVerificacao(string email, string nome, string codigo, string saudacao)
        {
            var userMailAddress = new MailAddress(email, nome);
            var emailSubject = "[TV Player] Código de Verificação";
            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background-color: #f8f9fa; padding: 20px;'>
                    
                    <!-- Header com Logo -->
                    <div style='background-color: #fdf7f7; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                        <a href='https://www.tvplayer.com.br' target='_blank'>
                            <img alt='TV Player Logo' src='https://www.tvplayer.com.br/email/png/logo.png' width='130' height='100' style='display: block; margin: 0 auto; font-family: Helvetica, Arial, sans-serif; color: #666666; font-size: 16px;' border='0'>
                        </a>
                    </div>
                    
                    <!-- Conteúdo Principal -->
                    <div style='background-color: #3498db; color: white; padding: 20px; text-align: center;'>
                        <h1 style='margin: 0; font-size: 24px; color: white;'>Código de Verificação</h1>
                    </div>
                    
                    <div style='background-color: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                        <p style='font-size: 16px; color: #2c3e50; margin-bottom: 20px; text-align: center;'>Olá <strong>{nome}</strong>, {saudacao}!</p>
                        <p style='font-size: 16px; color: #2c3e50; margin-bottom: 30px; text-align: center;'>Seu código de verificação da TV Player é:</p>
                        
                        <div style='background-color: #e74c3c; color: white; font-size: 36px; font-weight: bold; text-align: center; padding: 20px; border-radius: 10px; letter-spacing: 8px; margin: 30px 0; border: 3px solid #c0392b;'>
                            <span style='color: white; text-shadow: 1px 1px 2px rgba(0,0,0,0.3);'>{codigo}</span>
                        </div>
                        
                        <p style='font-size: 14px; color: #7f8c8d; text-align: center; margin-bottom: 20px;'>Este código é válido por <strong>5 minutos</strong>.</p>
                        <p style='font-size: 14px; color: #7f8c8d; text-align: center;'>Digite o código na tela para finalizar seu cadastro de demonstração.</p>
                        
                        <hr style='border: none; border-top: 1px solid #ecf0f1; margin: 30px 0;'>
                        
                        <!-- Footer -->
                        <div style='text-align: center; padding: 20px 0;'>
                            <p style='font-size: 12px; color: #95a5a6; margin: 0 0 10px 0;'>Esta é uma mensagem automática da TV Player. Não responda este email.</p>
                            <a href='https://www.tvplayer.com.br' target='_blank' style='color: #3498db; text-decoration: none; font-size: 14px;'>www.tvplayer.com.br</a>
                        </div>
                    </div>
                </div>";

            try
            {
                SmtpTVPlayer.Send(MailTVPlayer(
                    mailReplyToList: new List<MailAddress> { userMailAddress },
                    mailToList: new List<MailAddress> { userMailAddress },
                    mailBccList: new List<MailAddress> { contatoMailAddress },
                    subject: emailSubject,
                    body: emailBody,
                    isBodyHtml: true));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO EMAIL CODIGO VERIFICACAO] {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Envia email de código de verificação com retorno detalhado de erro
        /// </summary>
        public (bool sucesso, string erro) EnviarEmailCodigoVerificacaoComDetalhes(string email, string nome, string codigo, string saudacao)
        {
            try
            {
                bool resultado = EnviarEmailCodigoVerificacao(email, nome, codigo, saudacao);
                return (resultado, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Envia email de solicitação de demonstração com retorno detalhado de erro
        /// </summary>
        public (bool sucesso, string erro) EnviarEmailSolicitacaoDemonstracaoComDetalhes(string path, TabCliente cliente)
        {
            try
            {
                bool resultado = EnviarEmailSolicitacaoDemonstracao(path, cliente);
                return (resultado, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Envia email de demonstração existente com retorno detalhado de erro
        /// </summary>
        public (bool sucesso, string erro) EnviarEmailDemonstracaoExistenteComDetalhes(string path, TVPlayerSite.Models.CRM.Usuario usuario)
        {
            try
            {
                bool resultado = EnviarEmailDemonstracaoExistente(path, usuario);
                return (resultado, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Envia email de procedimento de instalação com retorno detalhado de erro
        /// </summary>
        public (bool sucesso, string erro) EnviarEmailProcedimentoInstalacaoComDetalhes(string path, UsuarioInfo usuario)
        {
            try
            {
                bool resultado = EnviarEmailProcedimentoInstalacao(path, usuario);
                return (resultado, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
