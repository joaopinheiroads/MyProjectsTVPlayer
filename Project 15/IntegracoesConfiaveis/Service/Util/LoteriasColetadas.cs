using System;
using System.Collections.Generic;
using System.Linq;
using TVPlayerAPI.Domain.Enums;

namespace TI.TVPlayer.Service.Util
{
    internal static class LoteriasColetadas
    {
        private static readonly IReadOnlyList<ELoteria> FORA_DE_USO = new List<ELoteria>
        {
            ELoteria.lotogol
        };

        internal static IEnumerable<ELoteria> Ativas
        {
            get { return Todas().Where(loteria => !FORA_DE_USO.Contains(loteria)); }
        }

        private static IEnumerable<ELoteria> Todas()
        {
            return Enum.GetValues(typeof(ELoteria)).Cast<ELoteria>();
        }
    }
}
