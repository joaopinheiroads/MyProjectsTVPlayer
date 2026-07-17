namespace TVPlayerSite.Configuration
{
  
    public sealed class SmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; } = 587;
        public string User { get; set; }
        public string Password { get; set; }
        public int TimeoutMs { get; set; } = 30000;


        public EnderecoEmail Site { get; set; } = new EnderecoEmail();
        public EnderecoEmail Contato { get; set; } = new EnderecoEmail();
        public EnderecoEmail Suporte { get; set; } = new EnderecoEmail();

        public sealed class EnderecoEmail
        {
            public string Email { get; set; }
            public string Nome { get; set; }
        }
    }
}
