using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Client.Dto
{
    public class ProdutoAddDTO
    {
        [Required]
        [MaxLength(100)]
        public string? Nome { get; set; }

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
        public bool Promocao { get; set; }

        [Column(TypeName = "decimal(19,2)")]
        public decimal? PrecoPromocional { get; set; }

        public DateTime? FimPromocao { get; set; }

        public string? imageUrlLoadedApi { get; set; }

        [Required]
        public bool Destaque { get; set; }

        [Required]
        public bool Ativo { get; set; }

        public List<int>? GroupsID { get; set; }

        public ICollection<string?>? ImagesID { get; set; }

        public ICollection<ProdutoImagemAddDTO>? ColProdutoImagem { get; set; }

        public ICollection<GrupoOrdenadoDTO>? GruposOrdenados { get; set; }

        public List<ProdutoHorarioAddDTO> Horarios { get; set; } = new List<ProdutoHorarioAddDTO>();

        public List<ProdutoPromocaoHorarioAddDTO> PromocoesPorHorario { get; set; } = new List<ProdutoPromocaoHorarioAddDTO>();

        public int EmpresaID { get; set; }
    }

    public class GrupoOrdenadoDTO
    {
        public int GrupoAdicionalID { get; set; }
        public int Posicao { get; set; }
    }
}
