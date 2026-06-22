using Cardápio.Client.Pages;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Infra.Model
{
    public class UsuarioEmpresa
    {
        public int ID { get; set; }

        public int EmpresaID { get; set; }

        [ForeignKey(nameof(EmpresaID))]
        public Company Empresa { get; set; }

        public int UsuarioID { get; set; }

        [ForeignKey(nameof(UsuarioID))]
        public User Usuario { get; set; }
    }
}
