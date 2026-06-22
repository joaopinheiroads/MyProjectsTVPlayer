using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Dto
{
    public class ProdutoUpdateDTO
    {          
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(400)]
        public string Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal? Preco { get; set; }

        [Required]
        public int? CategoriaID { get; set; }

        [Required]
        public bool Promocao { get; set; }

        [Column(TypeName = "decimal(19,2)")]
        public decimal? PrecoPromocional { get; set; }

        [Required]
        public bool Destaque { get; set; }

        public bool Ativo { get; set; }

        public ICollection<ProdutoImagemUpdateDTO> ColProdutoImagem { get; set; }
        public DateTime? FimPromocao { get; set; }
        public int QTDPessoa { get; set; }
        
        public List<ProdutoHorarioAddDTO> Horarios { get; set; } = new List<ProdutoHorarioAddDTO>();
        
        // Nova propriedade para promoções por horário
        public List<ProdutoPromocaoHorarioAddDTO> PromocoesPorHorario { get; set; } = new List<ProdutoPromocaoHorarioAddDTO>();

        // Garante que se Promocao for false, PrecoPromocional e FimPromocao sejam nulos
        public void NormalizePromotionFields()
        {
            if (!Promocao)
            {
                PrecoPromocional = null;
                FimPromocao = null;
            }
        }
    }
}
