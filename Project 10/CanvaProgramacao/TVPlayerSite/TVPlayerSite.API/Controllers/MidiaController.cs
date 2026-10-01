using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TVPlayer.CRUD.Models;
using TVPlayer.CRUD.Repositories;
using NReco.VideoInfo;
using TVPlayer.CRUD.Interfaces.Repositories;

namespace TVPlayerSite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MidiaController : ControllerBase
    {
        private readonly ILogger<MidiaController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMidiaRepository _midiaRepository;

        public MidiaController(ILogger<MidiaController> logger, IConfiguration configuration, IMidiaRepository midiaRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _midiaRepository = midiaRepository;
        }

        [HttpPost("upload")]
        [RequestSizeLimit(524_288_000)] // 500MB
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
            [FromForm] IFormFile file,
            [FromForm] int userId,
            [FromForm] string userName,
            [FromForm] string token,
            [FromForm] int Duracao,
            [FromForm] int width,
            [FromForm] int height)
        {
            try
            {
                Console.WriteLine("=== MIDIA CONTROLLER CHAMADO ===");
                Console.WriteLine($"File: {file?.FileName}");
                Console.WriteLine($"UserId: {userId}");
                Console.WriteLine($"UserName: {userName}");
                Console.WriteLine($"Duracao: {Duracao}");
                Console.WriteLine("=================================");

                if (string.IsNullOrWhiteSpace(token))
                    return Unauthorized(new { success = false, error = "Token não informado" });

                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, error = "Nenhum arquivo enviado" });

                _logger.LogInformation("Arquivo recebido. Nome: {FileName}, Tamanho: {FileSize}, Usuário: {UserId}",
                    file.FileName, file.Length, userId);

                var uploadsPath = _configuration["UploadPath"]
                    ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");

#if DEBUG
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);
#else
                if (!Directory.Exists(uploadsPath))
                {
                     Directory.CreateDirectory(uploadsPath);
                }
#endif

                var originalFileName = file.FileName;
                var displayFileName = originalFileName.Length >= 44
                    ? originalFileName.Substring(0, 43) + Path.GetExtension(originalFileName).ToLower()
                    : originalFileName;

                var extension = Path.GetExtension(file.FileName).ToLower();
                var tempFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, tempFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp" };
                var isImage = imageExtensions.Contains(extension);

                int duracao = Duracao > 0 ? Duracao : 15;
                int? largura = width > 0 ? (int?)width : null;
                int? altura = height > 0 ? (int?)height : null;

                string formato = extension.TrimStart('.');

                if (!isImage && largura == null)
                {
                    try
                    {
                        var ffProbe = new FFProbe();
                        var mediaInfo = ffProbe.GetMediaInfo(filePath);

                        foreach (var stream2 in mediaInfo.Streams)
                        {
                            if (stream2.CodecType == "video")
                            {
                                largura = stream2.Width;
                                altura = stream2.Height;
                                break;
                            }
                        }

                        duracao = Convert.ToInt32(mediaInfo.Duration.TotalSeconds);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Não foi possível extrair metadados: {FileName}", tempFileName);
                    }
                }

                string md5Hash = CalculateMD5(filePath);
                string md5FileName = md5Hash + extension;

                // Renomeia o arquivo para usar o hash MD5
                var md5FilePath = Path.Combine(uploadsPath, md5FileName);
                if (System.IO.File.Exists(md5FilePath))
                    System.IO.File.Delete(md5FilePath);
                System.IO.File.Move(filePath, md5FilePath);



                Video midia = new Video
                {
                    UsuarioIdcadastrou = userId,
                    Nome = displayFileName,
                    FileName = md5FileName,
                    DisplayFileName = displayFileName,
                    Ativo = true,
                    DataUpload = DateTime.Now,
                    Duracao = duracao,
                    Tamanho = Convert.ToInt32(file.Length) / 1024,
                    Largura = largura,
                    Altura = altura,
                    Formato = formato,
                    HasThumbnail = false,
                    Md5 = md5Hash
                };



                await _midiaRepository.AddMidiaAsync(midia);

                return Ok(new
                {
                    success = true,
                    message = "Arquivo recebido com sucesso",
                    videoId = midia.Id,
                    fileName = md5FileName,
                    nome = originalFileName,
                    md5 = md5Hash,
                    duracao,
                    size = file.Length,
                    userId,
                    userName,
                    savedPath = md5FilePath,
                    receivedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao receber arquivo");
                return StatusCode(500, new
                {
                    success = false,
                    error = "Erro ao processar arquivo",
                    details = ex.Message
                });
            }
        }

        private string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
