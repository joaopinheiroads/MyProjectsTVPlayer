using System.Threading.Tasks;

namespace TVPlayerSite.Disparos
{
   
    public interface IDisparoMensagem
    {
      
        Task<EnvioMensagemResultado> EnviarMensagemAsync(string number, string message);

  
       
        Task<EnvioMensagemResultado> EnviarTemplateAsync(string number, string templateId, bool comBotaoCopyCode, params string[] parametros);

       
        Task<EnvioMensagemResultado> EnviarTemplateComBotaoUrlAsync(string number, string templateId, string urlButtonParam, int urlButtonIndex = 0, params string[] parametrosBody);
    }

 
    public class EnvioMensagemResultado
    {
        public bool Sucesso { get; set; }
        public string Erro { get; set; }
    }
}
