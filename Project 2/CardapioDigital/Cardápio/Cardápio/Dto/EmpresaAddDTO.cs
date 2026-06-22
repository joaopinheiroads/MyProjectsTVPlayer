using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class EmpresaAddDTO
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [Required]
        [MaxLength(100)]
        public string QRCode { get; set; }

        [MaxLength(100)]
        public string RazaoSocial { get; set; }

        [MaxLength(18)]
        public string CNPJ { get; set; }

        [MaxLength(11)]
        public string Celular { get; set; }

        [MaxLength(10)]
        public string Telefone { get; set; }

        public int EmpresaTipoID { get; set; }

        public bool AtenderWhatsapp { get; set; }

        [Required]
        [MaxLength(8)]
        public string CEP { get; set; }

        [Required(ErrorMessage = "O Estado é obrigatório.")]
        public string EstadoID { get; set; }

        [Required(ErrorMessage = "A Cidade é obrigatória.")]
        public int CidadeID { get; set; }

        public ICollection<ImageFileAddDTO?>? ColBannerImagem { get; set; }

        public ICollection<ImageFileAddDTO?>? ColLogoImagem { get; set; }
    }
}
