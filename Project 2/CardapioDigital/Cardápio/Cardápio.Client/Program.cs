using Cardápio.Client.Components.SelectCompany;
using Cardápio.Client.Components.StateModal;
using Cardápio.Client.Pages.Cardapio.ShoppingCartContext;
using Cardápio.Client.Pages.Categoria;
using Cardápio.Client.Pages.Empresas;
using Cardápio.Client.Pages.Grupo;
using Cardápio.Client.Pages.Produto;
using Cardápio.Client.Pages.Usuarios;
using Cardápio.Client.Pages.Adicionais;
using Cardápio.Client.Pages.RecuperarSenha;
using Radzen;
using Cardápio.Client.Pages.Mesas;
using Cardápio.Client.Components.Modal;
using Cardápio.Client.Components.Modal.QrCodeDownload;
using Cardápio.Client.Pages.AuthClient.OptionsForm;
using Cardápio.Client.Pages.ConfirmarPedido;
using Cardápio.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cardápio.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCardapioClientServices(this IServiceCollection services)
        {
            services.AddScoped<SelectCompanyService>();
            services.AddScoped<StateContainer>();
            services.AddScoped<CategoriaService>();
            services.AddScoped<ProdutoService>();
            services.AddScoped<EmpresaService>();
            services.AddScoped<AdicionalService>();
            services.AddScoped<UsuarioService>();
            services.AddScoped<GrupoService>();
            services.AddScoped<ModalService>();
            services.AddScoped<MesasService>();
            services.AddScoped<ShoppingCartContextService>();
            services.AddScoped<QrCodeLayoutService>();
            services.AddScoped<CodeServiceFactory>();
            services.AddScoped<EmailCodeSenderService>();
            services.AddScoped<SmsCodeSenderService>();
            services.AddScoped<PedidoStorage>();
            services.AddScoped<ImageUploadService>();
            services.AddScoped<ProdutoHorarioService>();
            services.AddScoped<Cardápio.Client.Services.PromocaoService>();

            services.AddRadzenComponents();

            return services;
        }
    }
}
