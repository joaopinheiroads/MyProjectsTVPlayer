using System;

namespace TVPlayerAPI.G1.Classes
{
    internal class NoticiaG1
    {
        internal string Manchete { get; private set; }

        internal string Resumo { get; private set; }

        internal string Link { get; private set; }

        internal string Imagem { get; private set; }

        internal DateTime Publicacao { get; private set; }

        internal NoticiaG1(string manchete, string resumo, string link, string imagem, DateTime publicacao)
        {
            Manchete = manchete;
            Resumo = resumo;
            Link = link;
            Imagem = imagem;
            Publicacao = publicacao;
        }

        internal NoticiaG1 CompletadaCom(MateriaG1 materia)
        {
            return new NoticiaG1(Manchete, Resumo ?? materia.Resumo, Link, materia.Imagem ?? Imagem, Publicacao);
        }

        internal bool Completa
        {
            get { return Resumo != null && Imagem != null; }
        }
    }
}
