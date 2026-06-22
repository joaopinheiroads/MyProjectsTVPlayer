using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Dto
{
    public class LayoutGetDTO
    {
        public int ID { get; set; }

        public string Empresa { get; set; }

        public LogoGetDTO Logo { get; set; }

        public ICollection<BannerGetDTO> ColBanner { get; set; }
    }
}
