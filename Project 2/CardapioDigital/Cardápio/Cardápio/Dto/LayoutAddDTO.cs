namespace Cardápio.Dto
{
    public class LayoutAddDTO
    {
        public LogoAddDTO Logo { get; set; }
        public ICollection<BannerAddDTO> ColBanner { get; set; }
    }
}
