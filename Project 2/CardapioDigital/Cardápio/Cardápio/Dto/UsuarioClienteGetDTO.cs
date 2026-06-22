namespace Cardápio.Dto
{
    public class UsuarioClienteGetDTO
    {
        public int ID { get; set; }
        public string? Email { get; set; }
        public string? Nome { get; set; }
        public string? Celular { get; set; }
        public string? CPF { get; set; }
        public int UsuarioTipoID { get; set; }
    }
}
