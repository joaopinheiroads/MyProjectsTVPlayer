using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TVPlayerSite.Models.CRM;

namespace TVPlayerSite.Services.Demonstracao
{
    // Pacote de dados que fica "guardado" durante a verificação de código.
    public sealed class DadosVerificacao
    {
        public TabCliente Cliente { get; set; }
        public string Codigo { get; set; }
        public DateTime EmitidoEm { get; set; }
    }

    public interface IVerificacaoSessionStore
    {
        void Salvar(TabCliente cliente, string codigo, DateTime emitidoEm);
        void AtualizarCodigo(string codigo, DateTime emitidoEm);
        bool TryObter(out DadosVerificacao dados);
        void Limpar();
    }

    // Concentra TODO o acesso à sessão referente à demonstração num só lugar.
    // Antes o controller conhecia os nomes das chaves ("DemonstracaoTemp" etc.),
    // serializava/deserializava JSON e formatava datas espalhado em 5 actions.
    // Agora ele só conversa com este contrato.
    public sealed class VerificacaoSessionStore : IVerificacaoSessionStore
    {
        private const string ChaveCliente = "DemonstracaoTemp";
        private const string ChaveCodigo = "CodigoVerificacaoTemp";
        private const string ChaveTimestamp = "TimestampVerificacaoTemp";

        private readonly IHttpContextAccessor _http;

        public VerificacaoSessionStore(IHttpContextAccessor http) => _http = http;

        private ISession Sessao => _http.HttpContext.Session;

        public void Salvar(TabCliente cliente, string codigo, DateTime emitidoEm)
        {
            Sessao.SetString(ChaveCliente, JsonConvert.SerializeObject(cliente));
            Sessao.SetString(ChaveCodigo, codigo);
            Sessao.SetString(ChaveTimestamp, emitidoEm.ToString());
        }

        public void AtualizarCodigo(string codigo, DateTime emitidoEm)
        {
            Sessao.SetString(ChaveCodigo, codigo);
            Sessao.SetString(ChaveTimestamp, emitidoEm.ToString());
        }

        public bool TryObter(out DadosVerificacao dados)
        {
            dados = null;

            var clienteJson = Sessao.GetString(ChaveCliente);
            var codigo = Sessao.GetString(ChaveCodigo);
            var timestamp = Sessao.GetString(ChaveTimestamp);

            if (string.IsNullOrEmpty(clienteJson) ||
                string.IsNullOrEmpty(codigo) ||
                string.IsNullOrEmpty(timestamp))
            {
                return false;
            }

            if (!DateTime.TryParse(timestamp, out var emitidoEm))
                return false;

            dados = new DadosVerificacao
            {
                Cliente = JsonConvert.DeserializeObject<TabCliente>(clienteJson),
                Codigo = codigo,
                EmitidoEm = emitidoEm
            };
            return true;
        }

        public void Limpar()
        {
            Sessao.Remove(ChaveCliente);
            Sessao.Remove(ChaveCodigo);
            Sessao.Remove(ChaveTimestamp);
        }
    }
}
