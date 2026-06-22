using Cardápio.Dto;

namespace Cardápio.Infra.Services
{
    public class ServerImageUploadService
    {
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger _logger;

    public ServerImageUploadService(IWebHostEnvironment webHostEnvironment, ILogger logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<ImageUploadResponseDTO> SaveTempImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Arquivo não fornecido ou vazio");

                var extensao = Path.GetExtension(file.FileName).ToLower();
                // Gerar ID único para a imagem
                var imageId = Guid.NewGuid().ToString();
                var fileName = $"{imageId}{extensao}";

                // Diretório temporário para imagens
                var tempDir = Path.Combine(_webHostEnvironment.ContentRootPath, "imagens", "temp");
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
                
                return new ImageUploadResponseDTO
                {
                    ImageId = imageId,
                    ImageUrl = imageUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar imagem temporária");
                throw;
            }
        }

        public async Task<bool> DeleteTempImageAsync(string imageId)
        {
            try
            {
                var tempDir = Path.Combine(_webHostEnvironment.ContentRootPath, "imagens", "temp");
                var files = Directory.GetFiles(tempDir, $"{imageId}.*");
                
                foreach (var file in files)
                {
                    File.Delete(file);
                    _logger.LogInformation($"Imagem temporária removida: {file}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover imagem temporária com ID: {imageId}");
                return false;
            }
        }

        public async Task<string> MoveTempImageToProductAsync(string tempImageId, int productId)
        {
            try
            {
                var tempDir = Path.Combine(_webHostEnvironment.ContentRootPath, "imagens", "temp");
                var productDir = Path.Combine(_webHostEnvironment.ContentRootPath, "imagens", "produtos");
                Directory.CreateDirectory(productDir);
                
                var tempFiles = Directory.GetFiles(tempDir, $"{tempImageId}.*");
                
                if (tempFiles.Length == 0)
                    throw new FileNotFoundException($"Imagem temporária não encontrada: {tempImageId}");
                
                var tempFile = tempFiles[0];
                var tempFileName = Path.GetFileName(tempFile);
                var extension = Path.GetExtension(tempFileName);
                
                var newFileName = $"produto_{productId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var newFilePath = Path.Combine(productDir, newFileName);
                
                // Mover arquivo
                File.Move(tempFile, newFilePath);
                
                var imageUrl = $"/imagens/produtos/{newFileName}";
                _logger.LogInformation($"Imagem movida de temp para produto: {imageUrl}");
                
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao mover imagem temporária para produto. TempId: {tempImageId}, ProductId: {productId}");
                throw;
            }
        }

    public async Task<ImageUploadResponseDTO> SaveAdditionalImageAsync(IFormFile file, ILogger logger)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Arquivo não fornecido ou vazio");

                var extensao = Path.GetExtension(file.FileName).ToLower();
                var imageId = Guid.NewGuid().ToString();
                var fileName = $"{imageId}{extensao}";
                var additionalDir = Path.Combine(_webHostEnvironment.ContentRootPath, "imagens", "ImageAdditional");
                Directory.CreateDirectory(additionalDir);
                var filePath = Path.Combine(additionalDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var imageUrl = $"/imagens/ImageAdditional/{fileName}";
                logger.LogInformation($"Imagem de adicional salva: {imageUrl}");

                return new ImageUploadResponseDTO
                {
                    ImageId = fileName,
                    ImageUrl = imageUrl
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao salvar imagem de adicional");
                throw;
            }
        }
    }
}
