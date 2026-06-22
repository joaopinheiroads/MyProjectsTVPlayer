using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using TVPlayer.CRUD.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

using TVPlayerSite.API.Logs;







namespace TVPlayerSite.API.Disparos
{
    public class VerificadorDeDemonstracoes : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;
        private static bool _jaExecutouHoje = false;

        public VerificadorDeDemonstracoes(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Exclui arquivos de log do VerificadorDeDemonstracoes com mais de 30 dias
            ExcluirLogsAntigos();

            // Garante que o diretório de logs existe ao iniciar
            try
            {
                var logDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG DIR ERROR] {ex.Message}");
            }

            VerificadorDeDemonstracoesLogger.Log("VerificadorDeDemonstracoes iniciado.");

            // Executa a verificação imediatamente ao iniciar o debug
           // ExecutarVerificacao(null);

            // Configura o timer para executar periodicamente
            
            _timer = new Timer(ExecutarVerificacao, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(20));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            VerificadorDeDemonstracoesLogger.Log("VerificadorDeDemonstracoes parado.");
            _timer?.Change(Timeout.Infinite, 0); // Para o timer
            return Task.CompletedTask;
        }

        private async void ExecutarVerificacao(object state)
        {
            VerificadorDeDemonstracoesLogger.Log("Iniciando verificação de demonstrações.");

            try
            {
                var agora = DateTime.Now;

            #if DEBUG
                var hora = 25; // Hora de teste em DEBUG
            #else
                var hora = 10;
            #endif

                if (agora.Hour == hora && !_jaExecutouHoje ){

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<VideoContext>();

                        // Busca os usuários com 10 dias de demonstração
                        List <UsuarioTerminalDemonstracao> usuarios  = BuscarUsuariosCom10Dias(context);

                        if (usuarios.Any())
                        {
                            VerificadorDeDemonstracoesLogger.Log($"Usuários encontrados: {usuarios.Count}");
                            foreach (var usuario in usuarios)
                            {
                                VerificadorDeDemonstracoesLogger.Log($"Encontrado: Nome={usuario.usuarioNome}, Empresa={usuario.empresaNome}, Terminal={usuario.terminalNome}, Celular={usuario.Celular}, UltimoDia={usuario.UltimoDia}");
                            }
                            await VerificarClientesAsync(usuarios);
                        }
                        else
                        {
                            VerificadorDeDemonstracoesLogger.Log("Nenhum usuário encontrado com 10 dias de demonstração.");
                        }
                        _jaExecutouHoje = true; // Marca que já executou hoje
                    } 

                }  
                else if (agora.Hour != hora)
                {
                    _jaExecutouHoje = false; // Reseta para o próximo dia
                }
              
            }
            catch (Exception ex)
            {
                VerificadorDeDemonstracoesLogger.Log($"Erro durante a execução da verificação: {ex.Message}");
            }

            VerificadorDeDemonstracoesLogger.Log("Verificação de demonstrações concluída.");
        }




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


        private async Task VerificarClientesAsync(List<UsuarioTerminalDemonstracao> usuarios)
        {
            

            foreach (UsuarioTerminalDemonstracao usuarioTerminalDemonstracao in usuarios)
            {
                //if (usuarioTerminalDemonstracao.ID != 6861)

                //    continue;

                // Remova o filtro de teste se quiser logar todos

                try {
                    var mensagemPenultimo = $"Olá *{usuarioTerminalDemonstracao.usuarioNome}* - *{usuarioTerminalDemonstracao.empresaNome}*, {saudacao}, seu período de demonstração está chegando ao fim. Você pode solicitar a contratação através do *Painel de Controle* em *Gerenciamento*, *menu Contratar*.\n\nEm caso de dúvidas contate-nos.\n\nAh, se não quiser receber mais nossas comunicações, é só digitar *SAIR* a qualquer momento";
                    var mensagemUltimoDia = $"Olá *{usuarioTerminalDemonstracao.usuarioNome}* - *{usuarioTerminalDemonstracao.empresaNome}* {saudacao}!\n\nEstamos encerrando a inscrição para testes, mas caso ainda tenha interesse, apenas nos chamar que reativaremos sua conta!\n\nA TV Player agradece o contato e se coloca à disposição para futuras demandas!\nAté logo mais!\n\nAh, se não quiser receber mais nossas comunicações, é só digitar *SAIR* a qualquer momento";

                    string numeroDestino = usuarioTerminalDemonstracao.Celular;
                    //string numeroDestino = "5543000000000";

                    using (var httpClient = new HttpClient()) {
                        DisparosChat disparosChat = new DisparosChat();
                        string mensagem = usuarioTerminalDemonstracao.UltimoDia ? mensagemUltimoDia : mensagemPenultimo;
                        var resultadoEnvio = await disparosChat.EnviarMensagemAsync(numeroDestino, mensagem);
                        string numeroComercial = "1142101933";
                        string mensagemErro = $"Olá, tivemos uma Falha ao enviar mensagem de WhatsApp no *alerta de vencimento de demonstração do décimo dia* para o cliente *{usuarioTerminalDemonstracao.usuarioNome}* da empresa *{usuarioTerminalDemonstracao.empresaNome}* com o número *{numeroDestino}*. O erro retornado foi: {resultadoEnvio.Erro}. Por favor, verifique o status do número e entre em contato com o cliente para garantir que ele receba as informações sobre a demonstração. Obrigado!";
                        string mensagemErroDois = $"Olá, tivemos uma Falha ao enviar mensagem de WhatsApp no *alerta de vencimento de demonstração do penúltimo dia* para o cliente *{usuarioTerminalDemonstracao.usuarioNome}* da empresa *{usuarioTerminalDemonstracao.empresaNome}* com o número *{numeroDestino}*. O erro retornado foi: {resultadoEnvio.Erro}. Por favor, verifique o status do número e entre em contato com o cliente para garantir que ele receba as informações sobre a demonstração. Obrigado!";


                        if (resultadoEnvio.Sucesso) {
                            if (usuarioTerminalDemonstracao.UltimoDia) {
                                VerificadorDeDemonstracoesLogger.Log($"Mensagem de ÚLTIMO DIA enviada com sucesso para: Nome={usuarioTerminalDemonstracao.usuarioNome}, Empresa={usuarioTerminalDemonstracao.empresaNome}, Terminal={usuarioTerminalDemonstracao.terminalNome}, Telefone={numeroDestino}");
                            }
                            else {
                                VerificadorDeDemonstracoesLogger.Log($"Mensagem de PENÚLTIMO DIA enviada com sucesso para: Nome={usuarioTerminalDemonstracao.usuarioNome}, Empresa={usuarioTerminalDemonstracao.empresaNome}, Terminal={usuarioTerminalDemonstracao.terminalNome}, Telefone={numeroDestino}");
                            }
                        }
                        else {
                            // Log detalhado da falha
                            VerificadorDeDemonstracoesLogger.Log(
                                $"FALHA ao enviar mensagem para: Nome={usuarioTerminalDemonstracao.usuarioNome}, Empresa={usuarioTerminalDemonstracao.empresaNome}, Terminal={usuarioTerminalDemonstracao.terminalNome}, Telefone={numeroDestino}\nConteúdo da mensagem não enviada:\n{mensagem}\nErro: {resultadoEnvio.Erro}"
                            );

                                try {

                                    if (mensagem.Contains("encerrando", StringComparison.OrdinalIgnoreCase)) {
                                        await disparosChat.EnviarMensagemAsync(numeroComercial, mensagemErro);
                                    }
                                    else if (mensagem.Contains("fim", StringComparison.OrdinalIgnoreCase)) {
                                        await disparosChat.EnviarMensagemAsync(numeroComercial, mensagemErroDois);


                                    }
                                }
                                catch (Exception innerEx) {
                                    Console.WriteLine($"Falha ao notificar erro via WhatsApp: {innerEx.Message}");
                                }


                        }
                    }
                }
                catch (Exception ex) {
                    VerificadorDeDemonstracoesLogger.Log($"Erro ao enviar mensagem para {usuarioTerminalDemonstracao.usuarioNome}: {ex.Message}");
                }
            }
        }

        private List<UsuarioTerminalDemonstracao> BuscarUsuariosCom10Dias(VideoContext context)
        {


            try
            {
                DateTime hoje = DateTime.Now.Date;
                 List <UsuarioTerminalDemonstracao> listResultUsuario = new List<UsuarioTerminalDemonstracao>();

                // Busca todos os terminais do grupo 3 com suas relações
                List<Terminal> listTerminal = (
                    from terminal in context.Terminal.Include("TerminalQtdDiasDemonstracao")
                    join qtdDiasDemo in context.TerminalQtdDiasDemonstracao
                        on terminal.Id equals qtdDiasDemo.TerminalId into joinTable
                    from terminalQtdDemo in joinTable.DefaultIfEmpty()
                    where terminal.GrupoTi == 3
                    && terminal.Ativo==true
                    && terminal.DataInstalacao != null
                    select terminal
                   
                    
                ).ToList();

                foreach (var terminal in listTerminal)
                {
                    // Validação: Data de instalação é obrigatória
                    if (terminal.DataInstalacao == null)
                    {
                        VerificadorDeDemonstracoesLogger.Log($"[AVISO] Terminal {terminal.Id} não tem data de instalação.");
                        continue;
                    }

                    // Define quantidade de dias da demonstração. Padrão = 10
                    int diasDemonstracao = terminal.TerminalQtdDiasDemonstracao?
                        .FirstOrDefault()?.QtdDiasDemonstracao ?? 10;


                    // Calcula a diferença de dias desde a instalação (apenas data, sem hora)
                    int diasDesdeInstalacao = (DateTime.Now.Date - terminal.DataInstalacao.Value.Date).Days;

                    // Se bate com os dias de demonstração, busca usuários da mesma empresa
                    if ((int)diasDesdeInstalacao == diasDemonstracao)
                    {
                        // Último dia
                        List<UsuarioTerminalDemonstracao> listUsuario = (from usuario in context.Usuario
                                                                         join tterminal in context.Terminal on usuario.EmpresaId equals tterminal.EmpresaId
                                                                         where usuario.EmpresaId == terminal.EmpresaId && usuario.Ativo == true && tterminal.Id == terminal.Id
                                                                         select new UsuarioTerminalDemonstracao
                                                                         {
                                                                             usuarioNome = usuario.Nome,
                                                                             ID = usuario.Id,
                                                                             Celular = usuario.Celular,
                                                                             terminalNome = terminal.AliasCliente,
                                                                             empresaNome = usuario.Empresa.Nome,
                                                                             UltimoDia = true
                                                                         }).ToList();

                        listResultUsuario.AddRange(listUsuario);
                    }
                    else if ((int)diasDesdeInstalacao == diasDemonstracao - 1)
                    {
                        // Penúltimo dia
                        List<UsuarioTerminalDemonstracao> listUsuario = (from usuario in context.Usuario
                                                                         join tterminal in context.Terminal on usuario.EmpresaId equals tterminal.EmpresaId
                                                                         where usuario.EmpresaId == terminal.EmpresaId && usuario.Ativo == true && tterminal.Id == terminal.Id
                                                                         select new UsuarioTerminalDemonstracao
                                                                         {
                                                                             usuarioNome = usuario.Nome,
                                                                             ID = usuario.Id,
                                                                             Celular = usuario.Celular,
                                                                             terminalNome = terminal.AliasCliente,
                                                                             empresaNome = usuario.Empresa.Nome,
                                                                             UltimoDia = false
                                                                         }).ToList();

                        listResultUsuario.AddRange(listUsuario);
                    }
                }
                return listResultUsuario;
            }
            catch (Exception ex)
            {
                VerificadorDeDemonstracoesLogger.Log($"Erro ao buscar usuários com 10 dias de demonstração: {ex.Message}");
                return null;
            }
        }

        private void ExcluirLogsAntigos()
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDir))
                    return;

                var arquivos = Directory.GetFiles(logDir, "VerificadorDeDemonstracoes_*.log");
                foreach (var arquivo in arquivos)
                {
                    var info = new FileInfo(arquivo);
                    DateTime dataArquivo = info.CreationTimeUtc;
                    if (dataArquivo == DateTime.MinValue)
                        dataArquivo = info.LastWriteTimeUtc;

                    if (dataArquivo < DateTime.UtcNow.AddDays(-30))
                    {
                        try
                        {
                            File.Delete(arquivo);
                        }
                        catch (Exception ex)
                        {
                            // Apenas loga no console, não interrompe a rotina
                            Console.WriteLine($"Falha ao excluir log antigo: {arquivo} - {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao excluir logs antigos: {ex.Message}");
            }
        }

        public class UsuarioTerminalDemonstracao
        {
            public string usuarioNome;
            public int ID;
            public string Celular;
            public string terminalNome;
            public string empresaNome;
            public bool UltimoDia; // true = último dia, false = penúltimo dia

        }






        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
