using System.Threading.Tasks;
using TVPlayer.CRUD.Interfaces.Repositories;

namespace TVPlayerSite.API.Interfaces.UnitOfWork
{
    public interface IVideoUnitOfWork
    {
        IAgendamentoRepository AgendamentoRepository { get; }
        ICidadeClimatempoRepository CidadeClimatempoRepository { get; }
        ICidadeYahooClimaRepository CidadeYahooClimaRepository { get; }
        IEfeitoBarraRepository EfeitoBarraRepository { get; }
        IEfeitoRelogioRepository EfeitoRelogioRepository { get; }
        IEmpresaRepository EmpresaRepository { get; }
        IGrupoRepository GrupoRepository { get; }
        IGrupoRSSCanalRepository GrupoRSSCanalRepository { get; }
        IGrupoRSSPrecoRepository GrupoRSSPrecoRepository { get; }
        IGrupoRSSPromocaoRepository GrupoRSSPromocaoRepository { get; }
        IGrupoRSSRotativoRepository GrupoRSSRotativoRepository { get; }
        IGrupoVideosRepository GrupoVideosRepository { get; }
        ILayoutPlayerRepository LayoutPlayerRepository { get; }
        IProgramacaoRepository ProgramacaoRepository { get; }
        IProgramacaoVideosRepository ProgramacaoVideosRepository { get; }
        IRSSCanalRepository RSSCanalRepository { get; }
        IRSSPrecoRepository RSSPrecoRepository { get; }
        IRSSPromocaoRepository RSSPromocaoRepository { get; }
        IRSSRotativoRepository RSSRotativoRepository { get; }
        ISuporteRepository SuporteRepository { get; }
        ISuporteTerminalRepository SuporteTerminalRepository { get; }
        ITerminalCidadeClimaRepository TerminalCidadeClimaRepository { get; }
        ITerminalInfoRepository TerminalInfoRepository { get; }
        ITerminalInstanciaConfigRepository TerminalInstanciaConfigRepository { get; }
        ITerminalLayoutPlayerRepository TerminalLayoutPlayerRepository { get; }
        ITerminalLayoutRepository TerminalLayoutRepository { get; }
        ITerminalParametroRepository TerminalParametroRepository { get; }
        ITerminalRepository TerminalRepository { get; }
        ITerminalSuporteRepository TerminalSuporteRepository { get; }
        ITerminalTimelineRepository TerminalTimelineRepository { get; }
        ITimelineRepository TimelineRepository { get; }
        IUsuarioGrupoRepository UsuarioGrupoRepository { get; }
        IUsuarioProgramacaoRepository UsuarioProgramacaoRepository { get; }
        IUsuarioRepository UsuarioRepository { get; }
        IVideoRepository VideoRepository { get; }
        IVideoconfAgendamentosSiteRepository VideoconfAgendamentosSiteRepository { get; }
        IVideoconfAgendamentosSiteConfigRepository VideoconfAgendamentosSiteConfigRepository { get; }

        Task SaveChanges();
    }
}
