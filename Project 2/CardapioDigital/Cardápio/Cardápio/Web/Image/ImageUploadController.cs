using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cardápio.Infra.Services;
using Cardápio.Dto;

namespace Cardápio.Web.Image
{
    [Route("api/image")]
    [ApiController]
    public class ImageUploadController : ControllerBase
    {
        private readonly AppPathsService _appPathsService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ImageUploadController> _logger;

        public ImageUploadController(
            AppPathsService appPathsService, 
            IWebHostEnvironment webHostEnvironment,
            ILogger<ImageUploadController> logger)
        {
            _appPathsService = appPathsService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("upload-temp")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImageUploadResponseDTO>> UploadTempImage(
        [FromForm] UploadTempImageRequestDTO request)
        {
            var file = request.File;
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nenhum arquivo foi enviado.");
                }

                // Verificar se é uma imagem
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest("Apenas arquivos JPEG, JPG e PNG são permitidos.");
                }

                // Limitar tamanho (50MB)
                if (file.Length > 50 * 1024 * 1024)
                {
                    return BadRequest("O arquivo é muito grande. Máximo permitido: 50MB.");
                }

                // Gerar ID único para a imagem
                var imageId = Guid.NewGuid().ToString();
                var fileName = $"{imageId}_{file.FileName}";
                
                // Diretório temporário para imagens
                var imagensPath = _appPathsService.GetImagesDirectory();
                var tempDir = Path.Combine(imagensPath, "temp");
                Directory.CreateDirectory(tempDir);
                
                var filePath = Path.Combine(tempDir, fileName);
                
                // Salvar arquivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                // URL da imagem
                var imageUrl = $"/imagens/temp/{fileName}";
                
                _logger.LogInformation($"Imagem temporária salva: {imageUrl}");
                
                var result = new ImageUploadResponseDTO
                {
                    ImageId = imageId,
                    ImageUrl = imageUrl
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload de imagem temporária");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("delete-temp/{imageId}")]
        public ActionResult DeleteTempImage(string imageId)
        {
            try
            {
                if (string.IsNullOrEmpty(imageId))
                {
                    return BadRequest("ID de imagem temporária inválido.");
                }

                var imagensPath = _appPathsService.GetImagesDirectory();
                var tempDir = Path.Combine(imagensPath, "temp");
                var files = Directory.GetFiles(tempDir, $"{imageId}_*");
                
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                    _logger.LogInformation($"Imagem temporária removida: {file}");
                }
                
                return Ok("Arquivo temporário removido com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover arquivo temporário com ID: {imageId}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("upload-additional")]
        public async Task<ActionResult<ImageUploadResponseDTO>> UploadAdditionalImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nenhum arquivo foi enviado.");
                }

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest("Tipo de arquivo não suportado.");
                }

                var imageService = new ServerImageUploadService(_webHostEnvironment, _logger);
                    var result = await imageService.SaveAdditionalImageAsync(file, _logger);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer upload de imagem de adicional");
                return StatusCode(500, "Erro interno ao salvar imagem de adicional.");
            }
        }
    }
}
