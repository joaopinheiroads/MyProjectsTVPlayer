using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TVPlayer.CRUD.Models;
using TVPlayerSite.API.DTO;
using TVPlayerSite.API.Interfaces.UnitOfWork;

namespace TVPlayerSite.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramacaoController : ControllerBase
    {
        private const int TIPO_MIDIA_REPOSITORIO = 1;
        private const int TIPO_MIDIA_RSS = 2;
        private const int TIPO_MIDIA_CAMPANHA = 4;
        private const int DURACAO_PADRAO_IMAGEM = 15;
        private const int DURACAO_MINIMA_IMAGEM = 1;
        private const int DURACAO_MAXIMA_IMAGEM = 14400;
        private const string ITEM_MIDIA = "midia";
        private const string ITEM_RSS = "rss";
        private const string ITEM_CAMPANHA = "campanha";
        private const string ITEM_PLUGIN = "plugin";
        private static readonly string[] EXTENSOES_IMAGEM = { "png", "jpg", "jpeg", "jpe", "bmp", "gif", "webp" };

        private readonly IVideoUnitOfWork _uowVideo;
        private readonly ILogger<ProgramacaoController> _logger;

        public ProgramacaoController(IVideoUnitOfWork uowVideo, ILogger<ProgramacaoController> logger)
        {
            _uowVideo = uowVideo;
            _logger = logger;
        }

        private int UsuarioID => int.TryParse(User.FindFirst("Id")?.Value, out var id) ? id : 0;

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                if (UsuarioID == 0)
                    return Unauthorized(new { success = false, error = "Token sem identificação do usuário" });

                var programacoes = await _uowVideo.ProgramacaoRepository.GetProgramacoesByUsuarioIDAsync(UsuarioID);

                return Ok(new { success = true, programacoes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar programações do usuário {UsuarioID}", UsuarioID);
                return StatusCode(500, new { success = false, error = "Erro ao listar programações" });
            }
        }

        [HttpGet("{programacaoID:int}/midias")]
        public async Task<IActionResult> GetMidiasAsync(int programacaoID)
        {
            try
            {
                if (UsuarioID == 0)
                    return Unauthorized(new { success = false, error = "Token sem identificação do usuário" });

                if (!await ProgramacaoDoUsuarioAsync(programacaoID))
                    return StatusCode(403, new { success = false, error = "Programação indisponível para este usuário" });

                var itens = await _uowVideo.ProgramacaoVideosRepository.GetProgramacaoVideosOrdenadosByProgramacaoIDAsync(programacaoID);
                var midias = itens.Select((item, indice) => new
                {
                    posicao = indice + 1,
                    programacaoVideoId = item.Id,
                    nome = NomeDoItem(item),
                    tipo = TipoDoItem(item)
                }).ToList();

                return Ok(new { success = true, midias });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar as mídias da programação {ProgramacaoID}", programacaoID);
                return StatusCode(500, new { success = false, error = "Erro ao listar as mídias da programação" });
            }
        }

        [HttpPost("{programacaoID:int}/midia")]
        public async Task<IActionResult> InserirMidiaAsync(int programacaoID, [FromBody] InserirMidiaProgramacaoDTO midia)
        {
            try
            {
                if (UsuarioID == 0)
                    return Unauthorized(new { success = false, error = "Token sem identificação do usuário" });

                if (midia == null || midia.VideoId <= 0)
                    return BadRequest(new { success = false, error = "VideoId é obrigatório" });

                if (midia.Posicao < 1)
                    return BadRequest(new { success = false, error = "A posição deve ser 1 ou maior" });

                if (!await ProgramacaoDoUsuarioAsync(programacaoID))
                    return StatusCode(403, new { success = false, error = "Programação indisponível para este usuário" });

                var video = await _uowVideo.VideoRepository.GetVideoByIdAsync(midia.VideoId);
                if (video == null || video.Ativo != true)
                    return NotFound(new { success = false, error = "Mídia não encontrada" });

                if (video.UsuarioIdcadastrou != UsuarioID)
                    return StatusCode(403, new { success = false, error = "Mídia indisponível para este usuário" });

                if (EhImagem(video) && (midia.Duracao < DURACAO_MINIMA_IMAGEM || midia.Duracao > DURACAO_MAXIMA_IMAGEM))
                    return BadRequest(new { success = false, error = $"A duração da imagem deve ficar entre {DURACAO_MINIMA_IMAGEM} e {DURACAO_MAXIMA_IMAGEM} segundos" });

                var item = new ProgramacaoVideos
                {
                    ProgramacaoId = programacaoID,
                    VideoId = video.Id,
                    UsuarioIdcadastrou = UsuarioID,
                    DataRegistro = DateTime.Now,
                    Ativo = true,
                    TipoMidiaId = TIPO_MIDIA_REPOSITORIO,
                    Duracao = ResolverDuracao(video, midia.Duracao)
                };

                if (midia.Posicao.HasValue)
                    await PosicionarAsync(item, midia.Posicao.Value);
                else
                    item.Ordem = await _uowVideo.ProgramacaoVideosRepository.GetProximaOrdemAsync(programacaoID);

                await _uowVideo.ProgramacaoVideosRepository.AddAsync(item);
                await _uowVideo.SaveChanges();

                _logger.LogInformation(
                    "Mídia {VideoId} inserida na programação {ProgramacaoID} pelo usuário {UsuarioID} na ordem {Ordem} (posição pedida: {Posicao})",
                    video.Id, programacaoID, UsuarioID, item.Ordem, midia.Posicao);

                return Ok(new
                {
                    success = true,
                    programacaoVideoId = item.Id,
                    programacaoId = programacaoID,
                    videoId = video.Id,
                    ordem = item.Ordem,
                    duracao = item.Duracao
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir mídia na programação {ProgramacaoID}", programacaoID);
                return StatusCode(500, new { success = false, error = "Erro ao inserir a mídia na programação" });
            }
        }

        private async Task<bool> ProgramacaoDoUsuarioAsync(int programacaoID)
        {
            var programacoes = await _uowVideo.ProgramacaoRepository.GetProgramacoesByUsuarioIDAsync(UsuarioID);
            return programacoes.Any(p => p.Id == programacaoID);
        }

        private async Task PosicionarAsync(ProgramacaoVideos item, int posicao)
        {
            var itens = (await _uowVideo.ProgramacaoVideosRepository.GetProgramacaoVideosOrdenadosByProgramacaoIDAsync(item.ProgramacaoId)).ToList();
            itens.Insert(Math.Min(posicao, itens.Count + 1) - 1, item);

            for (var ordem = 0; ordem < itens.Count; ordem++)
                itens[ordem].Ordem = ordem;
        }

        private static string TipoDoItem(ProgramacaoVideos item)
        {
            switch (item.TipoMidiaId)
            {
                case null:
                case TIPO_MIDIA_REPOSITORIO:
                    return item.Video != null ? ITEM_MIDIA : ITEM_PLUGIN;
                case TIPO_MIDIA_RSS:
                    return ITEM_RSS;
                case TIPO_MIDIA_CAMPANHA:
                    return ITEM_CAMPANHA;
                default:
                    return ITEM_PLUGIN;
            }
        }

        private static string NomeDoItem(ProgramacaoVideos item)
        {
            switch (TipoDoItem(item))
            {
                case ITEM_MIDIA:
                    return item.Video.DisplayFileName ?? item.Video.Nome;
                case ITEM_RSS:
                    return item.Rssusuario?.Nome;
                case ITEM_CAMPANHA:
                    return item.Campanha?.Nome;
                default:
                    return null;
            }
        }

        private static int? ResolverDuracao(Video video, int? duracaoInformada)
        {
            if (!EhImagem(video))
                return video.Duracao;

            return duracaoInformada ?? DURACAO_PADRAO_IMAGEM;
        }

        private static bool EhImagem(Video video)
        {
            var formato = video.Formato?.TrimStart('.').ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(formato))
                return EXTENSOES_IMAGEM.Contains(formato);

            var extensao = Path.GetExtension(video.Nome)?.TrimStart('.').ToLowerInvariant();
            return !string.IsNullOrWhiteSpace(extensao) && EXTENSOES_IMAGEM.Contains(extensao);
        }
    }
}
