// Trecho do Startup.cs do TVPlayerSite — apenas o registro de DI do fluxo de demonstração.
// (Arquivo ilustrativo: no projeto real isto vive dentro de ConfigureServices, junto com
//  MVC, localização, rotas, cache etc.)
//
// Este trecho é o "antes x depois" mais direto da refatoração: as dependências que o
// controller antes criava com `new` (EmailHelper, DisparosDigisac, Random, DateTime.Now)
// e os valores que estavam hard-coded (5 minutos, defaults da licença) passam a ser
// resolvidos aqui, num único ponto de composição.

public void ConfigureServices(IServiceCollection services)
{
    // Client da API interna (TVPlayerSite.API)
    services.AddSingleton<ApiClient>(provider => Factory.Instance);

    // WhatsApp por trás de uma interface: o restante do código depende de IDisparoMensagem,
    // nunca da DisparosDigisac concreta. Trocar o provedor não toca em service nenhum.
    // Credenciais vêm da configuração — nunca do código.
    services.AddTransient<IDisparoMensagem>(_ =>
        new DisparosDigisac(
            Configuration["Digisac:BaseUrl"],
            Configuration["Digisac:Token"],
            Configuration["Digisac:ServiceId"]));

    // Options Pattern: validade do código e defaults da licença saem do appsettings.json,
    // em vez de ficarem como número mágico dentro do controller.
    services.Configure<DemonstracaoOptions>(Configuration.GetSection("Demonstracao"));

    // Relógio injetável — é o que torna a expiração do código testável sem Thread.Sleep.
    services.AddSingleton<IRelogio, RelogioSistema>();

    services.AddScoped<ICodigoVerificacaoService, CodigoVerificacaoService>();
    services.AddScoped<IVerificacaoSessionStore, VerificacaoSessionStore>();
    services.AddScoped<IDadosService, DadosService>();

    // SMTP configurado a partir do appsettings (antes: senha embutida no EmailHelper).
    var smtpOptions = Configuration.GetSection("Email").Get<SmtpOptions>();
    EmailHelper.Configure(smtpOptions);
    services.AddScoped<IEmailService, EmailService>();

    services.AddScoped<ICadastroClienteService, CadastroClienteService>();

    // Orquestrador do fluxo: é a única dependência "de negócio" do controller.
    services.AddScoped<IDemonstracaoAppService, DemonstracaoAppService>();

    // Log deixa de ser helper estático e vira serviço injetável.
    services.AddSingleton<IVerificacaoLogger, VerificacaoLogger>();

    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // Necessário para o VerificacaoSessionStore acessar a sessão fora do controller.
    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
}
