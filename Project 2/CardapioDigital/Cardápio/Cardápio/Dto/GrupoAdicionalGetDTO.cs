namespace Cardápio.Dto
{
    public class GrupoAdicionalGetDTO
    {
        public int ID { get; set; }

        public string? Nome { get; set; }

        public bool Ativo { get; set; }

        public int? EmpresaID { get; set; }

        public int? Minimo { get; set; }

        public int? Maximo { get; set; }

        public string? TipoNome { get; set; }

        public int TipoID { get; set; }

        public int? Posicao { get; set; }

        public ICollection<GrupoAdicionalItemGetDTO> ColGrupoAdicionalItem { get; set; }

        public ICollection<ProdutoGetDTO>? ColProducts { get; set; }
    }
}
