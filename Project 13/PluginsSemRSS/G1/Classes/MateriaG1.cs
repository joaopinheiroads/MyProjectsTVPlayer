namespace TVPlayerAPI.G1.Classes
{
    internal class MateriaG1
    {
        internal string Imagem { get; private set; }

        internal string Resumo { get; private set; }

        internal MateriaG1(string imagem, string resumo)
        {
            Imagem = imagem;
            Resumo = resumo;
        }
    }
}
