using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cardápio.Infra.Model
{
    public class QrCodeLayout
    {
        public int ID { get; set; }
        public string Nome { get; set; }
        public string CorTexto { get; set; }
        public string CorFundo { get; set; }
        public string CorBorda { get; set; }
        public int RadioBorda { get; set; }
        public string TextoTitulo { get; set; }
        public string TextoDescricao { get; set; }
        public bool Ativo { get; set; }

        [Column(TypeName = "datetime2(0)")]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DataCadastro { get; set; }

        [Column(TypeName = "datetime2(0)")]
        public DateTime? DataEdicao { get; set; }

        [Required]
        public int? EmpresaID { get; set; }

        [ForeignKey(nameof(EmpresaID))]
        public Company Empresa { get; set; }
    }
}
