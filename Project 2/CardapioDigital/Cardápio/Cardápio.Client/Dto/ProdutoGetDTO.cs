using Cardápio.Client.Pages.Cardapio.ShoppingCartContext;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Client.Dto
{
    public class ProdutoGetDTO
    {
        public int ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(200)]
        public string Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal? Preco { get; set; }

        [Required]
        public int? CategoriaID { get; set; }

        [Required]
        public int QTDPessoa { get; set; }

        [Required]
        [MaxLength(100)]
        public string Categoria { get; set; }

        [Required]
        public int? CategoriaOrdem { get; set; }

        public string ImagemThumbnail { get; set; }

        [Required]
        public bool Promocao { get; set; }

        [Column(TypeName = "decimal(19,2)")]
        public decimal? PrecoPromocional { get; set; }

        [Required]
        public bool Destaque { get; set; }

        [Required]
        public bool Ativo { get; set; }

        public List<string> ImagesID { get; set; }

        [Required]
        public DateTime DataCadastro { get; set; }

        public ICollection<GrupoAdicionalGetDTO> ColGrupoAdicional { get; set; }

        public ICollection<ProdutoHorarioGetDTO> Horarios { get; set; }

        public CartModel CartItem { get; set; }

        public DateTime? FimPromocao { get; set; }
        
        // Nova propriedade para promoções por dia e horário
        public List<ProdutoPromocaoHorarioGetDTO> PromocoesPorHorario { get; set; } = new();
    }

    public class ProdutoPromocaoHorarioGetDTO
    {
        public int ID { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFim { get; set; } = string.Empty;
        public decimal PrecoPromocional { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
