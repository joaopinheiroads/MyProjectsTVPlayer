using Cardápio.Infra.Interfaces.Repositories;

namespace Cardápio.Infra.Interfaces.UnitsOfWork
{
    public interface ICardapioUnitOfWork : IDisposable
    {
        IProdutoRepo ProdutoRepo { get; }
        IUsuarioRepo UsuarioRepo { get; }
        IEmpresaRepo EmpresaRepo { get; }
        IUsuarioEmpresaRepo UsuarioEmpresaRepo { get; }
        IUsuarioTipoRepo UsuarioTipoRepo { get; }
        ICategoriaRepo CategoriaRepo { get; }
        IEstadoRepo EstadoRepo { get; }
        ICidadeRepo CidadeRepo { get; }
        IProdutoImagemRepo ProdutoImagemRepo { get; }
        IProdutoThumbnailRepo ProdutoThumbnailRepo { get; }
        IGrupoAdicionalItemRepo GrupoAdicionalItemRepo { get; }
        IGrupoAdicionalItemImagemRepo GrupoAdicionalItemImagemRepo { get; }
        IGrupoAdicionalRepo GrupoAdicionalRepo { get; }
        ILayoutRepo LayoutRepo { get; }
        IBannerRepo BannerRepo { get; }
        ILogoRepo LogoRepo { get; }
        IGrupoRepo GrupoRepo { get; }
        ISolicitacaoRepo SolicitacaoRepo { get; }
        IGrupoUsuarioRepo GrupoUsuarioRepo { get; }
        IProdutoGrupoAdicionalRepo ProdutoGrupoAdicionalRepo { get; }
        IMesaRepo MesaRepo { get; }
        IQrCodeLayoutRepo QrCodeLayoutRepo { get; }
        IUsuarioStatusRepo UsuarioStatusRepo { get; }
        IUsuarioClienteRepo UsuarioClienteRepo { get; }
        ICodigoVerificacoRepo CodigoVerificacoRepo { get; }
        IPedidoRepo PedidoRepo { get; }
        IProdutoPromocaoHorarioRepo ProdutoPromocaoHorarioRepo { get; }
        Task Commit();
    }
}
