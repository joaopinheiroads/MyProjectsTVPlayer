using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Dto
{
    public class LayoutUpdateDTO
    {
        public LogoUpdateDTO Logo { get; set; }
        public ICollection<BannerUpdateDTO> ColBanner { get; set; }
    }
}
