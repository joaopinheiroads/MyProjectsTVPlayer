using Microsoft.Extensions.Configuration;

namespace Cardápio.Infra.Services
{
    public class AppPathsService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AppPathsService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public string GetImagesDirectory()
        {
            var relativePath = _configuration["AppPaths:ImagesDirectory"] ?? "imagens";
            return Path.Combine(_environment.ContentRootPath, relativePath);
        }

        public string GetLogosDirectory()
        {
            var relativePath = _configuration["AppPaths:LogosDirectory"] ?? "imagens/logo";
            return Path.Combine(_environment.ContentRootPath, relativePath);
        }

        public string GetBannersDirectory()
        {
            var relativePath = _configuration["AppPaths:BannersDirectory"] ?? "imagens/banner";
            return Path.Combine(_environment.ContentRootPath, relativePath);
        }

        public string GetProductImagesDirectory()
        {
            var relativePath = _configuration["AppPaths:ProductImagesDirectory"] ?? "imagens/ImageProducts";
            return Path.Combine(_environment.ContentRootPath, relativePath);
        }

        public string GetThumbnailsDirectory()
        {
            var relativePath = _configuration["AppPaths:ThumbnailsDirectory"] ?? "imagens/ThumbnailProducts";
            return Path.Combine(_environment.ContentRootPath, relativePath);
        }

        public string GetAdditionalImagesDirectory()
        {
            var relativePath = _configuration["AppPaths:AdditionalImagesDirectory"] ?? "imagens/ImageAdditional";
            return Path.Combine(_environment.ContentRootPath, relativePath);
        }

        public string GetQrCodeDirectory()
        {
            var relativePath = _configuration["AppPaths:QrCodeDirectory"] ?? "imagens/QrCode";
            return Path.Combine(_environment.ContentRootPath, relativePath);
        }

        // URLs para static files
        public string GetProductImagesUrl(string filename)
        {
            return $"/imagens/ImageProducts/{filename}";
        }

        public string GetThumbnailsUrl(string filename)
        {
            return $"/imagens/ThumbnailProducts/{filename}";
        }

        public string GetLogosUrl(string filename)
        {
            return $"/imagens/logo/{filename}";
        }

        public string GetBannersUrl(string filename)
        {
            return $"/imagens/banner/{filename}";
        }

        public string GetAdditionalImagesUrl(string filename)
        {
            return $"/imagens/ImageAdditional/{filename}";
        }

        public string GetQrCodeUrl(string filename)
        {
            return $"/imagens/QrCode/{filename}";
        }

        // Cria diretórios se não existirem
        public void EnsureDirectoriesExist()
        {
            var directories = new[]
            {
                GetImagesDirectory(),
                GetLogosDirectory(),
                GetBannersDirectory(),
                GetProductImagesDirectory(),
                GetThumbnailsDirectory(),
                GetAdditionalImagesDirectory(),
                GetQrCodeDirectory()
            };

            foreach (var dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        // Nenhuma alteração obrigatória aqui, pois o host é obtido diretamente via IHttpContextAccessor no QrCodeService.
        // Se desejar centralizar, pode criar um método como abaixo:

        public string GetCurrentHost(IHttpContextAccessor httpContextAccessor)
        {
            var request = httpContextAccessor.HttpContext?.Request;
            return request != null
                ? $"{request.Scheme}://{request.Host.Value}"
                : "https://localhost";
        }
    }
}
