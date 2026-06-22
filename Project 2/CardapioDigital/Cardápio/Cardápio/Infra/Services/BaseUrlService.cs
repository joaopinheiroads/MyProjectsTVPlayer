using Microsoft.Extensions.Configuration;

namespace Cardápio.Infra.Services
{
    public class BaseUrlService
    {
        private readonly IConfiguration _configuration;

        public BaseUrlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetBaseUrl()
        {
            return _configuration.GetValue<string>("BaseUrl") ?? 
                   _configuration.GetValue<string>("DefaultConnectionHttp") ?? 
                   "https://localhost:7243/";
        }

        public string GetImageUrl(string relativePath)
        {
            var baseUrl = GetBaseUrl().TrimEnd('/');
            var cleanPath = relativePath.TrimStart('/');
            return $"{baseUrl}/{cleanPath}";
        }
    }
}
