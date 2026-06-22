using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class EmpresaAddDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(100)]
        public string QRCode { get; set; }

        [StringLength(100, MinimumLength = 3)]
        public string RazaoSocial { get; set; }

        [StringLength(14, MinimumLength = 14)]
        public string CNPJ { get; set; }

        [StringLength(11, MinimumLength = 11)]
        public string Celular { get; set; }

        [StringLength(10, MinimumLength = 10)]
        public string Telefone { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 8)]
        public string CEP { get; set; }

        [Required]
        public string EstadoID { get; set; }

        public int EmpresaTipoID { get; set; }

        public int DiasDemo { get; set; }

        public bool Ativo { get; set; }

        public bool AtenderWhatsapp { get; set; }

        [Required]
        public int CidadeID { get; set; }

        public FilesDto? Files { get; set; }

        public string? imageLogoUrlLoadedApi { get; set; }

        public string? imageBannerUrlLoadedApi { get; set; }

        public ICollection<ImageFileAddDTO?>? ColBannerImagem { get; set; }

        public ICollection<ImageFileAddDTO?>? ColLogoImagem { get; set; }
    }
}
