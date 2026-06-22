using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Infra.Model
{
    public class Info
    {
        public int ID { get; set; }

        #region Endereco

        [Required]
        [Column(TypeName = "char")]
        [StringLength(8, MinimumLength = 8)]
        public string CEP { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(150)]
        public string Endereco { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(50)]
        public string Numero { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Complemento { get; set; }

        [Required]
        [Column(TypeName = "varchar")]
        [MaxLength(100)]
        public string Bairro { get; set; }

        [Required]
        [Column(TypeName = "char")]
        [StringLength(2, MinimumLength = 2)]
        public string EstadoID { get; set; }

        [Required]
        public int CidadeID { get; set; }

        #endregion

        #region Horario de Funcionamento

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime DomingoInicio1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime DomingoFim1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime DomingoInicio2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime DomingoFim2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SegundaInicio1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SegundaFim1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SegundaInicio2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SegundaFim2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime TercaInicio1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime TercaFim1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime TercaInicio2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime TercaFim2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime QuartaInicio1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime QuartaFim1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime QuartaInicio2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime QuartaFim2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime QuintaInicio1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime QuintaFim1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime QuintaInicio2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime QuintaFim2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SextaInicio1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SextaFim1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SextaInicio2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SextaFim2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SabadoInicio1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SabadoFim1 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SabadoInicio2 { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public DateTime SabadoFim2 { get; set; }

        #endregion

        [Required]
        public int? UsuarioIDCadastro { get; set; }

        [ForeignKey(nameof(UsuarioIDCadastro))]
        public User UsuarioCadastro { get; set; }

        public int? UsuarioIDEdicao { get; set; }

        [ForeignKey(nameof(UsuarioIDEdicao))]
        public User UsuarioEdicao { get; set; }

        [Column(TypeName = "datetime2(0)")]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DataCadastro { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime? DataEdicao { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DefaultValue(true)]
        public bool Ativo { get; set; }
    }
}
