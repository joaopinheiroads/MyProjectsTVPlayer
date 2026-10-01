namespace TVPlayer.CRUD.Classes
{
    public class ProgramacaoResumo
    {
        public ProgramacaoResumo(int id, string nome, string categoria)
        {
            Id = id;
            Nome = nome;
            Categoria = categoria;
        }

        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Categoria { get; private set; }
    }
}
