using CanvaAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CanvaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly ITVPlayerService _tvPlayerService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        ITVPlayerService tvPlayerService,
        IConfiguration configuration,
        ILogger<UploadController> logger)
    {
        _tvPlayerService = tvPlayerService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    [RequestSizeLimit(524_288_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload()
    {
      
        try
        {
            Console.WriteLine("Lendo FormData...");
            var form = await Request.ReadFormAsync();
            Console.WriteLine($"FormData lido. Keys: {string.Join(", ", form.Keys)}");
            
            var file = form.Files.GetFile("file");
            var userId = form["userId"].ToString();
            var userName = form["userName"].ToString();

            Console.WriteLine($"File: {file?.FileName ?? "NULL"}");
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"UserName: {userName}");

            var authHeader = Request.Headers.Authorization.ToString();
            var token = authHeader.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { success = false, error = "Autenticação necessária" });

            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, error = "Nenhum arquivo foi enviado" });

            var maxSizeMB = _configuration.GetValue<int>("FileUpload:MaxFileSizeMB", 500);
            if (file.Length > maxSizeMB * 1024 * 1024)
                return BadRequest(new { success = false, error = $"Arquivo muito grande. Máximo: {maxSizeMB}MB" });

            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>()
                ?? new[] { ".png", ".jpg", ".mp4" };

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new { success = false, error = "Tipo de arquivo não permitido" });

            _logger.LogInformation("Recebendo arquivo: {FileName}, Tamanho: {Size} bytes, Usuário: {UserId}",
                file.FileName, file.Length, userId);

            // A CanvaAPI é só intermediária: o arquivo é repassado em stream para o
            // TVPlayer e não fica guardado aqui. O identificador abaixo existe apenas
            // para log/resposta.
            var uniqueSuffix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" + Random.Shared.Next(1000000000);
            var fileId = $"{uniqueSuffix}-{file.FileName}";

            var width = int.TryParse(form["width"], out var w) ? w : 0;
            var height = int.TryParse(form["height"], out var h) ? h : 0;

            var imageExtensions = new[] { ".png", ".jpg" };
            var isImage = imageExtensions.Contains(fileExtension);

            // Imagem tem duração fixa (15s). Para vídeo, extrair a duração real do
            // arquivo via FFProbe — o Canva não envia duração no arquivo exportado,
            // e mandar 0 fazia o vídeo não entrar na playlist do TVPlayer.
            var duracao = 15;
            if (!isImage)
            {
                // O FFProbe exige um caminho físico, então gravamos o vídeo num
                // temporário só para ler a duração e apagamos logo em seguida.
                var tempPath = Path.Combine(Path.GetTempPath(), $"canvaapi-{Guid.NewGuid():N}{fileExtension}");
                try
                {
                    using (var stream = new FileStream(tempPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var ffProbe = new NReco.VideoInfo.FFProbe();
                    var mediaInfo = ffProbe.GetMediaInfo(tempPath);
                    duracao = Convert.ToInt32(mediaInfo.Duration.TotalSeconds);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível extrair a duração do vídeo: {FileName}", file.FileName);
                    duracao = 0;
                }
                finally
                {
                    try
                    {
                        if (System.IO.File.Exists(tempPath))
                            System.IO.File.Delete(tempPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Não foi possível remover o temporário: {Path}", tempPath);
                    }
                }
            }

            Console.WriteLine("=== ANTES DE CHAMAR TVPLAYER API ===");
            Console.WriteLine($"Chamando TVPlayerService.UploadMediaAsync");
            Console.WriteLine($"UserId: {userId}");
            Console.WriteLine($"UserName: {userName}");
            Console.WriteLine($"Duracao: {duracao}");
            Console.WriteLine("=====================================");

            var result = await _tvPlayerService.UploadMediaAsync(
                file,
                userId,
                userName,
                token,
                duracao,
                width,
                height
            );

            Console.WriteLine("=== DEPOIS DE CHAMAR TVPLAYER API ===");
            Console.WriteLine($"Result Success: {result?.Success}");
            Console.WriteLine($"Result Error: {result?.Error}");
            Console.WriteLine("======================================");

            // Se o TVPlayer recusou o arquivo, NÃO retornar 200. Antes
            // retornávamos Ok() sempre que o arquivo era salvo localmente, então
            // o Canva via "sucesso" mesmo com o TVPlayer rejeitando o upload —
            // PNG/vídeo sumiam silenciosamente. Agora propagamos a falha.
            if (result == null || !result.Success)
            {
                _logger.LogWarning("TVPlayer recusou o upload: {Error}", result?.Error);
                return StatusCode(502, new
                {
                    success = false,
                    error = result?.Error ?? "Falha ao enviar para o TVPlayer",
                    fileId,
                    originalName = file.FileName,
                    tvPlayerResult = result
                });
            }

            return Ok(new
            {
                success = true,
                fileId,
                originalName = file.FileName,
                size = file.Length,
                userId,
                timestamp = DateTime.UtcNow,
                tvPlayerResult = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Upload endpoint");
            return StatusCode(500, new { success = false, error = "Erro ao processar o arquivo" });
        }
    }

    // Os endpoints GET (listar) e DELETE de arquivos foram removidos junto com a
    // pasta uploads: expunham as mídias publicamente e não eram usados pelo app.

    [HttpPost("metadata")]
    public async Task<IActionResult> UploadMetadata()
    {
        try
        {
            var form = await Request.ReadFormAsync();

            var imageId = form["imageId"].ToString();
            var width = int.TryParse(form["width"], out var w) ? w : 0;
            var height = int.TryParse(form["height"], out var h) ? h : 0;
            var rotation = int.TryParse(form["rotation"], out var r) ? r : 0;

            _logger.LogInformation(
                "Metadata recebido — ImageId: {ImageId}, Width: {Width}, Height: {Height}, Rotation: {Rotation}",
                imageId, width, height, rotation);

            // Use os valores aqui: salvar no banco, repassar para TVPlayer, etc.

            return Ok(new { success = true, imageId, width, height, rotation });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao receber metadados");
            return StatusCode(500, new { success = false, error = "Erro ao processar metadados" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "ok",
            timestamp = DateTime.UtcNow,
            service = "CanvaAPI Upload Service"
        });
    }
}
