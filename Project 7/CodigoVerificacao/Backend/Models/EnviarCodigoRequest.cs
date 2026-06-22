using System;
using System.Collections.Generic;
using System.Text;

namespace TVPlayerSite.Models.CRM
{
    public class EnviarCodigoRequest
    {
        public string Email { get; set; }
        public string Nome { get; set; }
        public string Codigo { get; set; }
    }
}
