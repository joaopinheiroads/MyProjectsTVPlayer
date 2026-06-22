using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cardápio.Client.Dto;

namespace Cardápio.Dto
{
    public class ProdutoAddDTO
    {
        // Adicione a propriedade Id para compatibilidade com o fluxo de edição/criação
        public int? Id { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(400)]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O Preço é obrigatório.")]
        [Column(TypeName = "decimal(19,2)")]
        public decimal? Preco { get; set; }

        [Required]
        public int QTDPessoa { get; set; }

        [Required(ErrorMessage = "A categoria é obrigatória.")]
        public int? CategoriaID { get; set; }

        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public bool Promocao { get; set; }

        [Column(TypeName = "decimal(19,2)")]
        public decimal? PrecoPromocional { get; set; }

        public string? imageUrlLoadedApi { get; set; }

        [Required]
        public bool Destaque { get; set; }

        public bool Ativo { get; set; }

        public ICollection<string?> ImagesID { get; set; } = new List<string?>();

        public List<int> GroupsID { get; set; } = new List<int>();

        public ICollection<ProdutoImagemAddDTO> ColProdutoImagem { get; set; } = new List<ProdutoImagemAddDTO>();

        public ICollection<GrupoOrdenadoDTO> GruposOrdenados { get; set; } = new List<GrupoOrdenadoDTO>();

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

    public class GrupoOrdenadoDTO
    {
        public int GrupoAdicionalID { get; set; }
        public int Posicao { get; set; }
    }
}
