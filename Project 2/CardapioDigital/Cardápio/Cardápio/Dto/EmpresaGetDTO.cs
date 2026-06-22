using System.ComponentModel.DataAnnotations;

namespace Cardápio.Dto
{
    public class EmpresaGetDTO
    {
        public int ID { get; set; }

        [MaxLength(100)]
        public string Nome { get; set; }

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

        public string ImageBanner { get; set; }
        
        public string ImageLogo { get; set; }

        public int EmpresaTipoID { get; set; }

        public bool Ativo { get; set; }

        public bool? Excluido { get; set; }

        public bool AtenderWhatsapp { get; set; }

        public DateTime DataCadastro { get; set; }

        [MaxLength(8)]
        public string CEP { get; set; }

        public string EstadoID { get; set; }

        public EstadoGetDTO Estado { get; set; }

        public int CidadeID { get; set; }

        public int? DiasDemo { get; set; }

        public CidadeGetDTO Cidade { get; set; }

        public List<UsuarioGetDTO>? Usuarios { get; set; }
    }
}
