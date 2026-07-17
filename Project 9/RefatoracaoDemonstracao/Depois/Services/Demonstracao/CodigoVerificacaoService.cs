using System;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using TVPlayerSite.Configuration;
using TVPlayerSite.Services.Abstractions;

namespace TVPlayerSite.Services.Demonstracao
{
    public interface ICodigoVerificacaoService
    {
        string Gerar();

        bool EstaValido(DateTime emitidoEm);
    }

    public sealed class CodigoVerificacaoService : ICodigoVerificacaoService
    {
        private readonly IRelogio _relogio;
        private readonly int _minutosValidade;

        public CodigoVerificacaoService(IRelogio relogio, IOptions<DemonstracaoOptions> options)
        {
            _relogio = relogio;
            _minutosValidade = options.Value.MinutosValidadeCodigo;
        }

        public string Gerar()
        {
            const int min = 1000;
            const int maxInclusive = 9999;
            int valor = ProximoInteiro(min, maxInclusive);
            return valor.ToString();
        }

        public bool EstaValido(DateTime emitidoEm)
        {
            return _relogio.Agora.Subtract(emitidoEm).TotalMinutes <= _minutosValidade;
        }

        private static int ProximoInteiro(int min, int maxInclusive)
        {
            uint faixa = (uint)(maxInclusive - min + 1);
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                uint valor = BitConverter.ToUInt32(bytes, 0);
                return (int)(min + (valor % faixa));
            }
        }
    }
}
