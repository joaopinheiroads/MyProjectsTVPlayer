using Microsoft.AspNetCore.Hosting;

namespace Cardápio.Services
{
    public class BaseUrlService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public BaseUrlService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public string GetBaseUrl()
        {
            // Em desenvolvimento, usar localhost:7243
            if (_environment.IsDevelopment())
            {
                return "https://localhost:7243";
            }

            // Em produção, usar configuração ou fallback
            return _configuration.GetValue<string>("BaseUrl") ?? 
                   _configuration.GetValue<string>("DefaultConnectionHttp") ?? 
                   "https://localhost:7243";
        }

        public string GetImageUrl(string imageType, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var baseUrl = GetBaseUrl();
            return $"{baseUrl}/imagens/{imageType}/{fileName}";
        }

        public string GetBannerImageUrl(string fileName)
        {
            return GetImageUrl("banner", fileName);
        }

        public string GetLogoImageUrl(string fileName)
        {
            return GetImageUrl("logo", fileName);
        }
    }
}
