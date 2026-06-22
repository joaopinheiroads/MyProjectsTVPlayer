using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class ProductTransferDataDTO
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string? Nome { get; set; }

        [Required]
        [MaxLength(400)]
        public string? Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal? Preco { get; set; }

        [Required]
        public int? CategoriaID { get; set; }

        [Required]
        public int QTDPessoa { get; set; }

        [Required]
        public bool? Promocao { get; set; }

        [Column(TypeName = "decimal(19,2)")]
        public decimal? PrecoPromocional { get; set; }

        [Required]
        public bool? Destaque { get; set; }

        [Required]
        public bool Ativo { get; set; }

        public ICollection<string> ImagesID { get; set; }

        public string? imageUrlLoadedApi { get; set; }

        [Required]
        public int EmpresaID { get; set; }

        public Stream? ImageFile { get; set; }

        public List<ProdutoHorarioAddDTO> Horarios { get; set; } = new List<ProdutoHorarioAddDTO>();
        public DateTime? FimPromocao { get; set; }
        
        // Nova propriedade para promoções por dia e horário
        public List<ProdutoPromocaoHorarioAddDTO> PromocoesPorHorario { get; set; } = new List<ProdutoPromocaoHorarioAddDTO>();
    }

    public class ProdutoPromocaoHorarioAddDTO
    {
        [Required]
        public string DiaSemana { get; set; } = string.Empty;
        
        [Required]
        public string HoraInicio { get; set; } = string.Empty;
        
        [Required] 
        public string HoraFim { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal PrecoPromocional { get; set; }
    }

    public class ProductTransferDTO
    {
        public ProductTransferDataDTO ProductTransferDataDTO { get; set; }
        public List<GrupoAdicionalGetDTO> GroupsAdditionalGetDTO { get; set; }
    }
}
