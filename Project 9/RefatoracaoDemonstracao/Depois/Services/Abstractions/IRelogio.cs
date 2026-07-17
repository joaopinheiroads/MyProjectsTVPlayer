using System;

namespace TVPlayerSite.Services.Abstractions
{
    public interface IRelogio
    {
        DateTime Agora { get; }
    }

    public sealed class RelogioSistema : IRelogio
    {
        public DateTime Agora => DateTime.Now;
    }
}


