using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TVPlayerSite.Models.CRM; 

namespace TVPlayerSite.API.Disparos
{
    public class DisparoDemonstracao : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;
        private static bool _jaExecutouWhatsAppHoje = false;

        // Injete o IServiceProvider para criar scopes quando necessário
        public DisparoDemonstracao(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("DisparoDemonstracao iniciado.");
            
            // TESTE - Execução imediata comentada (vai executar apenas às 8h)
            // ExecutarVerificacao(null);
            
            // Timer verifica a cada 10 minutos se chegou a hora (8h) de executar
            _timer = new Timer(ExecutarVerificacao, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("DisparoDemonstracao parado.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void ExecutarVerificacao(object state)
        {
            try
            {
                await ExecutarVerificacaoDisparosAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na verificação de disparos: {ex.Message}");
            }
        }

        // Função principal que verifica horário e executa disparos
        public async Task ExecutarVerificacaoDisparosAsync()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Verificando se deve executar WhatsApp. Flag atual: {_jaExecutouWhatsAppHoje}");
            
            var horaAtual = DateTime.Now.Hour;
            

            #if DEBUG
            var horaExecucao = 25; // Hora de teste em DEBUG
            #else
            var horaExecucao = 10; // 10h da manhã

            #endif

            // Executa apenas às 10h da manhã e apenas uma vez por dia
            if (horaAtual == horaExecucao && !_jaExecutouWhatsAppHoje)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Horário correto (8h) - Iniciando execução de WhatsApp...");
                _jaExecutouWhatsAppHoje = true; // Marca como executado ANTES de executar
                await VerificarEEnviarWhatsAppAsync();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Execução de WhatsApp concluída.");
            }
            else if (horaAtual != horaExecucao)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Fora do horário de execução. Atual: {horaAtual}h, Necessário: {horaExecucao}h");
            }
            else
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] WhatsApp já foi executado hoje às {horaExecucao}h. Pulando...");
            }

            // Reset do flag à meia-noite
            if (horaAtual == 0)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Resetando flag - nova execução permitida amanhã às {horaExecucao}h.");
                _jaExecutouWhatsAppHoje = false;
            }
        }

        // Busca clientes cadastrados nos últimos 2 dias e sem e-mail verificado
        public async Task<List<TabCliente>> BuscarClientesParaWhatsAppAsync()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var crmContext = scope.ServiceProvider.GetRequiredService<CRMContext>();
                    var dataLimite = DateTime.Now.AddDays(-2); // Últimos 2 dias
                    
                    var clientes = new List<TabCliente>();
                    
                    // Usar ADO.NET diretamente para ter controle sobre valores NULL
                    var connectionString = crmContext.Database.GetDbConnection().ConnectionString;
                    
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        
                        var sql = @"SELECT cliID, cliNome, cliEmail, cliTelefoneCelular, 
                                           cliDataCadastro, cliEmailVerified, cliAtivo
                                    FROM tabCliente 
                                    WHERE cliDataCadastro >= @dataLimite 
                                      AND cliEmailVerified IS NULL 
                                      AND DisparoWhatsApp IS NULL
                                      AND cliTelefoneCelular IS NOT NULL 
                                      AND cliTelefoneCelular != ''
                                    ORDER BY cliDataCadastro DESC";
                        
                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@dataLimite", dataLimite);
                            
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var cliente = new TabCliente
                                    {
                                        CliId = reader.IsDBNull(reader.GetOrdinal("cliID")) ? 0 : reader.GetInt32(reader.GetOrdinal("cliID")),
                                        CliNome = reader.IsDBNull(reader.GetOrdinal("cliNome")) ? string.Empty : reader.GetString(reader.GetOrdinal("cliNome")),
                                        CliEmail = reader.IsDBNull(reader.GetOrdinal("cliEmail")) ? string.Empty : reader.GetString(reader.GetOrdinal("cliEmail")),
                                        CliTelefoneCelular = reader.IsDBNull(reader.GetOrdinal("cliTelefoneCelular")) ? string.Empty : reader.GetString(reader.GetOrdinal("cliTelefoneCelular")),
                                        CliDataCadastro = reader.IsDBNull(reader.GetOrdinal("cliDataCadastro")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("cliDataCadastro")),
                                        CliEmailVerified = reader.IsDBNull(reader.GetOrdinal("cliEmailVerified")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("cliEmailVerified")),
                                        CliAtivo = reader.IsDBNull(reader.GetOrdinal("cliAtivo")) ? (bool?)true : reader.GetBoolean(reader.GetOrdinal("cliAtivo"))
                                    };
                                    
                                    clientes.Add(cliente);
                                }
                            }
                        }
                    }
                    
                    Console.WriteLine($"Encontrados {clientes.Count} clientes para WhatsApp");
                    return clientes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar clientes: {ex.Message}");
                return new List<TabCliente>();
            }
        }

        // Envia WhatsApp para todos os clientes encontrados
        public async Task VerificarEEnviarWhatsAppAsync()
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Buscando clientes para WhatsApp...");
                var clientes = await BuscarClientesParaWhatsAppAsync();

                if (clientes.Count == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Nenhum cliente encontrado para envio.");
                    return;
                }

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Iniciando envio para {clientes.Count} clientes...");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var disparosChat = scope.ServiceProvider.GetRequiredService<DisparosChat>();

                    for (int i = 0; i < clientes.Count; i++)
                    {
                        var cliente = clientes[i];
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Enviando para cliente {i + 1}/{clientes.Count}: {cliente.CliNome}");
                        await EnviarWhatsAppDemonstracaoAsync(cliente, disparosChat);
                    }

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] WhatsApp enviado para {clientes.Count} clientes.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Erro ao enviar WhatsApp para clientes: {ex.Message}");
                throw;
            }
        }

        // Envia mensagem de WhatsApp específica para lembrar da confirmação de email
        public async Task EnviarWhatsAppDemonstracaoAsync(TabCliente cliente, DisparosChat disparosChat)
        {
            try
            {
                var mensagem = $"Olá, {cliente.CliNome}! 👋\n\n" +
                              "Verificamos que você iniciou o teste da nossa plataforma de mídia indoor, mas não chegou a explorar todo o potencial dela.\n\n" +
                              "Com a TV Player, a sua TV se transforma em um canal de comunicação para aumentar as vendas, engajar colaboradores, clientes e visitantes — de forma simples e totalmente online.\n\n" +
                              "Gostaria de dar sequência para que você possa conhecer melhor os recursos e ver na prática como pode ajudar sua empresa?\n\n" +
                              "👉 É rápido, e pode fazer a diferença na sua comunicação.\n\n" +
                              "Alguns exemplos de aplicação 👇🏼\n\n" +
                              "https://www.instagram.com/tvplayer.com.br/";

                // Usar o número real do cliente (formatado para WhatsApp)
                string numeroCliente = "55" + cliente.CliTelefoneCelular; // Adiciona código do Brasil (55)
                var resultado = await disparosChat.EnviarMensagemAsync(numeroCliente, mensagem);

                if (resultado.Sucesso)
                {
                    Console.WriteLine($"WhatsApp enviado com sucesso para {numeroCliente} - Cliente: {cliente.CliNome}");
                    
                    // Marcar cliente como já tendo recebido WhatsApp
                    await MarcarDisparoWhatsAppAsync(cliente.CliId);
                }
                else
                {
                    Console.WriteLine($"Erro ao enviar WhatsApp para {numeroCliente} - Cliente: {cliente.CliNome}: {resultado.Erro}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar WhatsApp para cliente {cliente.CliEmail}: {ex.Message}");
            }
        }

        // Marca o cliente como já tendo recebido WhatsApp
        private async Task MarcarDisparoWhatsAppAsync(int clienteId)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var crmContext = scope.ServiceProvider.GetRequiredService<CRMContext>();
                    var connectionString = crmContext.Database.GetDbConnection().ConnectionString;
                    
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        
                        var sql = @"UPDATE tabCliente 
                                   SET DisparoWhatsApp = 1 
                                   WHERE CliId = @clienteId";
                        
                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@clienteId", clienteId);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
                
                Console.WriteLine($"Cliente ID {clienteId} marcado como DisparoWhatsApp = 1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao marcar disparo para cliente {clienteId}: {ex.Message}");
            }
        }
    }
}

