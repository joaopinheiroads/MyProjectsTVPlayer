using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using Cardápio.Infra.Services;

namespace Cardápio.Web.Imagem
{
    [ApiController]
    [Route("api/imagem")]
    public class ImagemController : ControllerBase
    {
        private readonly AppPathsService _appPathsService;

        public ImagemController(AppPathsService appPathsService)
        {
            _appPathsService = appPathsService;
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> BaixarImagem(string id)
        {
            try
            {
                // Construir o caminho do arquivo usando AppPathsService
                var qrCodeDirectory = _appPathsService.GetQrCodeDirectory();
                var filePath = Path.Combine(qrCodeDirectory, id);
                

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound($"Arquivo não encontrado: {id}");
                }

                var imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);

                using (var image = SixLabors.ImageSharp.Image.Load(imageBytes))
                {
                    using (var ms = new MemoryStream())
                    {
                        image.SaveAsPng(ms);
                        ms.Seek(0, SeekOrigin.Begin);

                        return File(ms.ToArray(), "image/png", "QRCode.png");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }
    }
}
