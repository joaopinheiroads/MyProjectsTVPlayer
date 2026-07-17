using System.Globalization;
using System.Text.RegularExpressions;

namespace TVPlayerSite.Services.Demonstracao
{
    public interface IDadosService
    {
        string NormalizarBrasil(string telefone);
        string FormatarNomePessoa(string nome);
        string FormatarNomeEmpresa(string empresa);
    }

    public class DadosService : IDadosService
    {
        // Normaliza o telefone para o formato que o WhatsApp Business exige (com DDI).
        // Lógica por TAMANHO (não por prefixo) para não confundir com o DDD 55 (RS):
        //  - 10 dígitos (DDD + 8) ou 11 (DDD + 9) = número local -> prefixa "55".
        //  - 12/13 dígitos = já tem o código do país -> mantém.
        public string NormalizarBrasil(string telefone)
        {
            string n = Regex.Replace(telefone ?? "", "[^0-9]", "");
            if (n.Length == 10 || n.Length == 11)
                n = "55" + n;
            return n;
        }

        // Nome de pessoa: limpa o espaço e padroniza o caixa, porque o cadastro costuma vir
        // gritando do banco ("JOAO DA SILVA"). Sem o ToLower antes, o ToTitleCase trata palavra
        // toda em maiúscula como sigla e devolve ela intacta.
        public string FormatarNomePessoa(string nome)
        {
            string limpo = LimparEspacos(nome);
            if (limpo.Length == 0)
                return "Cliente";

            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(limpo.ToLower());
        }

        // Nome de empresa: só limpa o espaço, sem mexer no caixa. Title-case aqui estragaria
        // marca com maiúscula no meio ou sigla: "TVPlayer" viraria "Tvplayer" e "JBL" viraria "Jbl".
        public string FormatarNomeEmpresa(string empresa)
        {
            return LimparEspacos(empresa);
        }

        // Tira espaço das pontas e colapsa espaço repetido do meio. O espaço na ponta é o que
        // quebra o negrito do WhatsApp: em "*João Lucas *" o "*" de fechamento não é aplicado.
        private static string LimparEspacos(string texto)
        {
            return Regex.Replace((texto ?? "").Trim(), @"\s+", " ");
        }
    }
}
