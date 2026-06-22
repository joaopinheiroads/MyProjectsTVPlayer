using System;
using System.Collections.Generic;

namespace TVPlayer.CRUD.Models
{
    public partial class TerminalQtdDiasDemonstracao
    {
        public int Id { get; set; }
        public int TerminalId { get; set; }
        public int QtdDiasDemonstracao { get; set; }

        public virtual Terminal Terminal { get; set; }
    }
}
