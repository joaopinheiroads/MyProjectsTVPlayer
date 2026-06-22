using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Interfaces.UnitsOfWork;
using Cardápio.Infra.Repositories;

namespace Cardápio.Infra.Helpers
{
    public class CardapioUnitOfWork : ICardapioUnitOfWork
    {
        private readonly AppDbContext context;

        private IUsuarioEmpresaRepo usuarioEmpresaRepo { get; set; }
        private IProdutoRepo produtoRepo = null;
        private IUsuarioRepo usuarioRepo = null;
        private IEmpresaRepo empresaRepo = null;
        private IUsuarioTipoRepo usuarioTipoRepo = null;
        private ICategoriaRepo categoriaRepo = null;
        private IEstadoRepo estadoRepo = null;
        private ICidadeRepo cidadeRepo = null;
        private ISolicitacaoRepo solicitacaoRepo = null;
        private IProdutoImagemRepo produtoImagemRepo = null;
        private IProdutoThumbnailRepo produtoThumbnailRepo = null;
        private ILayoutRepo layoutRepo = null;
        private IBannerRepo bannerRepo = null;
        private ILogoRepo logoRepo = null;
        private IGrupoRepo grupoRepo = null;
        private IGrupoUsuarioRepo grupoUsuarioRepo = null;
        private IGrupoAdicionalItemRepo grupoAdicionalItemRepo = null;
        private IGrupoAdicionalRepo grupoAdicionalRepo = null;
        private IGrupoAdicionalItemImagemRepo grupoAdicionalItemImagemRepo = null;
        private IProdutoGrupoAdicionalRepo produtoGrupoAdicionalRepo = null;
        private IMesaRepo mesaRepo = null;
        private IQrCodeLayoutRepo qrCodeLayoutRepo = null;
        private IUsuarioStatusRepo usuarioStatusRepo = null;
        private IUsuarioClienteRepo usuarioClienteRepo = null;
        private ICodigoVerificacoRepo codigoVerificacoRepo = null;
        private IPedidoRepo pedidoRepo = null;
        private IProdutoPromocaoHorarioRepo produtoPromocaoHorarioRepo = null;
        private IProdutoHorarioRepo produtoHorarioRepo = null;

        public CardapioUnitOfWork(AppDbContext context)
        {
            this.context = context;
        }

        public async Task Commit()
        {
            await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public IProdutoRepo ProdutoRepo
        {
            get
            {
                return produtoRepo ??= new ProdutoRepo(context);
            }
        }

        public IUsuarioEmpresaRepo UsuarioEmpresaRepo
        {
            get
            {
                return usuarioEmpresaRepo ??= new UsuarioEmpresaRepo(context);
            }
        }

        public IUsuarioRepo UsuarioRepo
        {
            get
            {
                return usuarioRepo ??= new UsuarioRepo(context);
            }
        }

        public IEmpresaRepo EmpresaRepo
        {
            get
            {
                return empresaRepo ??= new EmpresaRepo(context);
            }
        }

        public IUsuarioTipoRepo UsuarioTipoRepo
        {
            get
            {
                return usuarioTipoRepo ??= new UsuarioTipoRepo(context);
            }
        }

        public ICategoriaRepo CategoriaRepo
        {
            get
            {
                return categoriaRepo ??= new CategoriaRepo(context);
            }
        }

        public IEstadoRepo EstadoRepo
        {
            get
            {
                return estadoRepo ??= new EstadoRepo(context);
            }
        }

        public ICidadeRepo CidadeRepo
        {
            get
            {
                return cidadeRepo ??= new CidadeRepo(context);
            }
        }

        public IProdutoImagemRepo ProdutoImagemRepo
        {
            get
            {
                return produtoImagemRepo ??= new ProdutoImagemRepo(context);
            }
        }

        public IProdutoThumbnailRepo ProdutoThumbnailRepo
        {
            get
            {
                return produtoThumbnailRepo ??= new ProdutoThumbnailRepo(context);
            }
        }

        public ILayoutRepo LayoutRepo
        {
            get
            {
                return layoutRepo ??= new LayoutRepo(context);
            }
        }

        public IBannerRepo BannerRepo
        {
            get
            {
                return bannerRepo ??= new BannerRepo(context);
            }
        }

        public ILogoRepo LogoRepo
        {
            get
            {
                return logoRepo ??= new LogoRepo(context);
            }
        }

        public IGrupoRepo GrupoRepo
        {
            get
            {
                return grupoRepo ??= new GrupoRepo(context);
            }
        }

        public ISolicitacaoRepo SolicitacaoRepo
        {
            get
            {
                return solicitacaoRepo ??= new SolicitacaoRepo(context);
            }
        }

        public IGrupoUsuarioRepo GrupoUsuarioRepo
        {
            get
            {
                return grupoUsuarioRepo ??= new GrupoUsuarioRepo(context);
            }
        }

        public IGrupoAdicionalItemRepo GrupoAdicionalItemRepo
        {
            get
            {
                return grupoAdicionalItemRepo ??= new GrupoAdicionalItemRepo(context);
            }
        }

        public IGrupoAdicionalItemImagemRepo GrupoAdicionalItemImagemRepo
        {
            get
            {
                return grupoAdicionalItemImagemRepo ??= new GrupoAdicionalItemImagemRepo(context);
            }
        }

        public IGrupoAdicionalRepo GrupoAdicionalRepo
        {
            get
            {
                return grupoAdicionalRepo ??= new GrupoAdicionalRepo(context);
            }
        }

        public IProdutoGrupoAdicionalRepo ProdutoGrupoAdicionalRepo
        {
            get
            {
                return produtoGrupoAdicionalRepo ??= new ProdutoGrupoAdicionalRepo(context);
            }
        }

        public IMesaRepo MesaRepo
        {
            get
            {
                return mesaRepo ??= new MesaRepo(context);
            }
        }

        public IQrCodeLayoutRepo QrCodeLayoutRepo
        {
            get
            {
                return qrCodeLayoutRepo ??= new QrCodeLayoutRepo(context);
            }
        }

        public IUsuarioStatusRepo UsuarioStatusRepo
        {
            get
            {
                return usuarioStatusRepo ??= new UsuarioStatusRepo(context);
            }
        }

        public IUsuarioClienteRepo UsuarioClienteRepo
        {
            get
            {
                return usuarioClienteRepo ??= new UsuarioClienteRepo(context);
            }
        }

        public ICodigoVerificacoRepo CodigoVerificacoRepo
        {
            get
            {
                return codigoVerificacoRepo ??= new CodigoVerificacaoRepo(context);
            }
        }

        public IPedidoRepo PedidoRepo
        {
            get
            {
                return pedidoRepo ??= new PedidoRepo(context);
            }
        }

        public IProdutoPromocaoHorarioRepo ProdutoPromocaoHorarioRepo
        {
            get
            {
                return produtoPromocaoHorarioRepo ??= new ProdutoPromocaoHorarioRepo(context);
            }
        }

        public IProdutoHorarioRepo ProdutoHorarioRepo
        {
            get
            {
                return produtoHorarioRepo ??= new ProdutoHorarioRepo(context);
            }
        }
    }
}
