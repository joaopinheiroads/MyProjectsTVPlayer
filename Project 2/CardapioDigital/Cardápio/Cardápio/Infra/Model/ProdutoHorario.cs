using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Infra.Model
{
    [Table("ProdutoHorario")]
    public class ProdutoHorario
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ProdutoID { get; set; }

        [Required]
        [MaxLength(8)]
        public string HoraInicio { get; set; } = "";

        [Required]
        [MaxLength(8)]
        public string HoraFim { get; set; } = "";

        [Required]
        [MaxLength(20)]
        public string DiaSemana { get; set; } = ""; // Segunda,Terça,Quarta,Quinta,Sexta,Sábado,Domingo

        public bool Ativo { get; set; } = true;

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public DateTime? DataEdicao { get; set; }

        public int UsuarioIDCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        // Navigation Properties
        [ForeignKey("ProdutoID")]
        public virtual Product? Produto { get; set; }

        [ForeignKey("UsuarioIDCadastro")]
        public virtual User? UsuarioCadastro { get; set; }

        [ForeignKey("UsuarioIDEdicao")]
        public virtual User? UsuarioEdicao { get; set; }
    }
}
