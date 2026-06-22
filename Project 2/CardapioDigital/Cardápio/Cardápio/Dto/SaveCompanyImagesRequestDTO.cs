namespace Cardápio.Dto
{
    public class SaveCompanyImagesRequestDTO
    {
        public IFormFile? Banner { get; set; }
        public IFormFile? Logo { get; set; }
    }
}
