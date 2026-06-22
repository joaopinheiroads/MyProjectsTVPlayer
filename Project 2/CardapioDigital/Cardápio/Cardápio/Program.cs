using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Radzen;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Server.IISIntegration;
using Cardápio.Client.Components.SelectCompany;
using Microsoft.AspNetCore.DataProtection;
using Cardápio.Client.Components.StateModal;
using Cardápio.Client.Pages.Categoria;
using Cardápio.Client.Pages.Empresas;
using Cardápio.Client.Pages.Grupo;
using Cardápio.Client.Pages.Produto;
using Cardápio.Client.Pages.Usuarios;
using Cardápio.Client.Infra.Provider;
using Cardápio.Client.Infra.Crypto;
using Cardápio.Infra.Helpers;
using Cardápio.Domain.Category;
using Cardápio.Domain.Enterprise;
using Cardápio.Domain.Group;
using Cardápio.Domain.Product;
using Cardápio.Domain.ProdutoHorario;
using Cardápio.Domain.Promocao;
using Cardápio.Domain.PromocaoHorario;
using Cardápio.Domain.ProdutoPromocaoHorario;
using Cardápio.Domain.QrCode;
using Cardápio.Domain.User;
using Cardápio.Domain.Auth;
using Cardápio.Infra.Data;
using Cardápio.Client.Pages.Cardapio.ShoppingCartContext;
using Cardápio.Domain.AdditionalGroup;
using Cardápio.Client.Pages.Adicionais;
using Cardápio.Client.Pages.Mesas;
using Cardápio.Domain.MesaService;
using Cardápio.Client.Components.Modal;
using Cardápio.Client.Components.Modal.QrCodeDownload;
using Cardápio.Client.Pages.AuthClient.OptionsForm;
using Cardápio.Domain.Codigo;
using Cardápio.Domain.Pedido;
using Cardápio.Client.Pages.ConfirmarPedido;
using Cardápio.Client.Pages.RecuperarSenha;
using Cardápio.Hubs.PedidoHub;
using Cardápio.Infra.Services;
using Cardápio.Infra.Repositories;
using Microsoft.Extensions.FileProviders;
using Cardápio.Domain;


var builder = WebApplication.CreateBuilder(args);

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
Console.OutputEncoding = Encoding.UTF8;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .EnableSensitiveDataLogging()
           .LogTo(Console.WriteLine, LogLevel.Information));

// Adicione esta linha para registrar IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
    .SetApplicationName("menuKeys");

builder.Services.AddTransient<AuthService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<ProdutoHorarioService>();
builder.Services.AddScoped<ProdutoHorarioRepo>();
builder.Services.AddScoped<ProdutoPromocaoHorarioService>();
builder.Services.AddScoped<ProdutoPromocaoHorarioRepo>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<Crypto>();
builder.Services.AddScoped<EnterpriseService>();
builder.Services.AddScoped<ShoppingCartContextService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<GrupoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ModalService>();
builder.Services.AddScoped<AdditionalGroupService>();
builder.Services.AddScoped<MesasService>();
builder.Services.AddScoped<MesaService>();
builder.Services.AddScoped<QrCodeLayoutService>();
builder.Services.AddScoped<CodeServiceFactory>();
builder.Services.AddScoped<EmailCodeSenderService>();
builder.Services.AddScoped<SmsCodeSenderService>();
builder.Services.AddScoped<CodigoVerificacaoService>();
builder.Services.AddScoped<CardapioUnitOfWork>();
builder.Services.AddScoped<Validator>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<AdicionalService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<SelectCompanyService>();
builder.Services.AddScoped<QrCodeService>();
builder.Services.AddScoped<StateContainer>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<PedidoStorage>();
builder.Services.AddScoped<DisparosChatService>();
builder.Services.AddScoped<BaseUrlService>();
builder.Services.AddSingleton<AppPathsService>();
builder.Services.AddScoped<Cardápio.Client.Services.ImageUploadService>();
builder.Services.AddScoped<Cardápio.Client.Services.ProdutoHorarioService>();




builder.Services.AddSignalR(options =>
{
    // Configurações otimizadas para IIS
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
    options.StreamBufferCapacity = 10;
    options.EnableDetailedErrors = false; // Desabilitar em produção
}).AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cardápio API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        In = ParameterLocation.Header,
        Description = "Enter your Bearer token in the format **'Bearer {token}'**"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});
builder.Services.AddRadzenComponents();

builder.Services.AddScoped<ThemeService>();

builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();

    var handler = new HttpClientHandler
    {
        UseProxy = false,
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };

    var baseAddress = builder.Configuration.GetValue<string>("DefaultConnectionHttp");

    if (string.IsNullOrEmpty(baseAddress))
    {
        throw new InvalidOperationException("Base address is not configured correctly.");
    }

    Console.WriteLine(baseAddress);

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(baseAddress),
        Timeout = TimeSpan.FromMinutes(5)
    };
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<Cardápio.Services.BaseUrlService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "defaultSecretKey");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

//builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents(options =>
{
    // Configurar timeouts para Blazor Server
    options.DetailedErrors = builder.Environment.IsDevelopment();
})
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

// Configurações adicionais para IIS e produção
builder.Services.Configure<CircuitOptions>(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
});

// Configurar IIS Integration se disponível
if (builder.Environment.IsProduction())
{
    builder.Services.Configure<IISOptions>(options =>
    {
        options.AutomaticAuthentication = false;
        options.ForwardClientCertificate = false;
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cardápio API v1");
        c.RoutePrefix = "api-docs";
    });
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// CORS - deve vir antes dos arquivos estáticos
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

// Middleware silencioso - logs removidos para limpeza
app.Use(async (context, next) =>
{
    await next();
});

// Configurar para servir arquivos estáticos da pasta wwwroot (padrão) - SEGUNDO
app.UseStaticFiles();

// Configurar para servir arquivos estáticos da pasta imagens - TERCEIRO
var appPathsService = app.Services.GetRequiredService<AppPathsService>();
appPathsService.EnsureDirectoriesExist();

var imagensPath = appPathsService.GetImagesDirectory();
// Logs removidos para limpeza

if (Directory.Exists(imagensPath))
{
    var staticFileOptions = new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagensPath),
        RequestPath = "/imagens",
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/octet-stream",
        OnPrepareResponse = ctx =>
        {
            // Logs removidos para limpeza
            
            // Adicionar headers CORS para imagens
            ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "*";
            ctx.Context.Response.Headers["Access-Control-Allow-Methods"] = "GET, OPTIONS";
            ctx.Context.Response.Headers["Access-Control-Allow-Headers"] = "*";
            
            // Cache headers para imagens
            ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=3600";
        }
    };
    
    app.UseStaticFiles(staticFileOptions);
    // Logs removidos para limpeza

    // Teste se o FileProvider funciona - verificar arquivo específico
    var fileInfo = staticFileOptions.FileProvider.GetFileInfo("ImageProducts/930ce026-f47c-4b3d-9eb8-e4496ba07209.jpg");
    // Logs removidos para limpeza
    
    // Teste alternativo - verificar se a pasta ImageProducts é acessível pelo FileProvider
    var dirInfo = staticFileOptions.FileProvider.GetDirectoryContents("ImageProducts");
    // Logs removidos para limpeza
    
    // Listar conteúdo da pasta ImageProducts para debug
    var imageProductsPath = Path.Combine(imagensPath, "ImageProducts");
    if (Directory.Exists(imageProductsPath))
    {
        var files = Directory.GetFiles(imageProductsPath).Take(3);
       
    }
    
    // Teste específico para QrCode
    var qrCodePath = Path.Combine(imagensPath, "QrCode");
    
    if (Directory.Exists(qrCodePath))
    {
        var qrFiles = Directory.GetFiles(qrCodePath).Take(3);
       
    }
    
    // Teste específico para Banner
    var bannerPath = Path.Combine(imagensPath, "Banner");
    
    if (Directory.Exists(bannerPath))
    {
        var bannerFiles = Directory.GetFiles(bannerPath).Take(3);
       
        
        // Teste específico do arquivo mencionado no problema
        var specificBannerFile = Path.Combine(bannerPath, "c07c6f43-fc8a-49ec-91c6-8f71e88e2436.png");
        
        if (File.Exists(specificBannerFile))
        {
            var bannerFileInfo = new System.IO.FileInfo(specificBannerFile);
            
        }
    }
    
    // Teste específico para Logo
    var logoPath = Path.Combine(imagensPath, "Logo");
  
    if (Directory.Exists(logoPath))
    {
        var logoFiles = Directory.GetFiles(logoPath).Take(3);
        
        
        
        // Teste específico do arquivo mencionado no problema
        var specificLogoFile = Path.Combine(logoPath, "03d449b9-d2d9-4fbb-87eb-7186e0214eb1.png");
       
        if (File.Exists(specificLogoFile))
        {
            var logoFileInfo = new System.IO.FileInfo(specificLogoFile);
           
        }
    }
}
else
{
    Console.WriteLine("[STATIC FILES DEBUG] ERRO: Diretório de imagens não encontrado!");
}

// Middleware de redirecionamento - logs removidos para limpeza
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();
    
    // Lista de caminhos permitidos
    var allowedPaths = new[]
    {
        "/loja",
        "/imagens", 
        "/wwwroot",
        "/api",
        "/login",
        "/cardapios",
        "/_framework",
        "/_blazor",
        "/css",
        "/js",
        "/recuperarSenha",
         "/swagger"
    };

    // Verificar se o caminho atual é permitido
    bool isAllowed = allowedPaths.Any(allowedPath => 
        path?.StartsWith(allowedPath) == true) || path != "/";

    if (isAllowed)
    {
        await next();
        return;
    }

    // Apenas redirecionar a raiz "/" para login
    context.Response.Redirect("/login");
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Configurações WebSockets otimizadas para IIS
var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(15)
};
app.UseWebSockets(webSocketOptions);

app.MapRazorComponents<Cardápio.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Cardápio.Client.Services.ImageUploadService).Assembly);
app.MapRazorPages();
app.MapControllers();
app.MapHub<PedidoHub>("/pedidoHub");

app.Run();
