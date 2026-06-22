using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Infra.Model
{
    public class ProdutoGrupoAdicional
    {
        public int? ProdutoID { get; set; }

        [ForeignKey(nameof(ProdutoID))]
        public Product Produto { get; set; }

        public int? GrupoAdicionalID { get; set; }

        [ForeignKey(nameof(GrupoAdicionalID))]
        public GrupoAdicional GrupoAdicional { get; set; }

        public bool Ativo { get; set; }

        public int Posicao { get; set; }
    }
}
