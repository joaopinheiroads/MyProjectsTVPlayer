namespace TVPlayerSite.Configuration
{
    public sealed class DemonstracaoOptions
    {
        public int MinutosValidadeCodigo { get; set; } = 5;

        public LicencaDefaults Licenca { get; set; } = new LicencaDefaults();

        public sealed class LicencaDefaults
        {
            public int Validade { get; set; } = 96;
            public int UnidadeId { get; set; } = 1;
            public int UsuarioIdCadastro { get; set; } = 1;
            public int TipoDeLicencaId { get; set; } = 5;
            public int CliTipoId { get; set; } = 4;
        }
    }
}
