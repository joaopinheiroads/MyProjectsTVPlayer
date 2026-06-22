using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Cardápio.Dto;
using Cardápio.Infra.Model;
using Cardápio.Infra.Repositories;

namespace Cardápio.Infra.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //if (System.Diagnostics.Debugger.IsAttached == false)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}

            foreach (IMutableProperty property in modelBuilder.Model.GetEntityTypes().
                        SelectMany(x => x.GetProperties()).Where(x => x.Name == "DataCadastro"))
            {
                property.SetDefaultValueSql("getdate()");
            }

            foreach (IMutableProperty property in modelBuilder.Model.GetEntityTypes().
                        SelectMany(x => x.GetProperties()).Where(x => x.Name == "Ativo"))
            {
                property.SetDefaultValue(true);
            }

            foreach (IMutableProperty property in modelBuilder.Model.GetEntityTypes().
                        SelectMany(x => x.GetProperties()).Where(x => x.ClrType == typeof(string)))
            {
                //property.SetColumnType("varchar");
            }

            foreach (IMutableForeignKey foreignKey in modelBuilder.Model.GetEntityTypes().
                        SelectMany(x => x.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            #region Usuario          

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColProdutoCadastro)
                .WithOne(p => p.UsuarioCadastro)
                .HasForeignKey(p => p.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColProdutoEdicao)
                .WithOne(p => p.UsuarioEdicao)
                .HasForeignKey(p => p.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColCategoriaCadastro)
                .WithOne(c => c.UsuarioCadastro)
                .HasForeignKey(c => c.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColCategoriaEdicao)
                .WithOne(c => c.UsuarioEdicao)
                .HasForeignKey(c => c.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Grupo)
                .WithMany(g => g.ColUsers)
                .HasForeignKey(u => u.GrupoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<User>()
                .HasMany(g => g.ColGrupoCadastro)
                .WithOne(g => g.UsuarioCadastro)
                .HasForeignKey(g => g.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColGrupoEdicao)
                .WithOne(g => g.UsuarioEdicao)
                .HasForeignKey(g => g.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColGrupoUsuarioCadastro)
                .WithOne(gu => gu.UsuarioCadastro)
                .HasForeignKey(gu => gu.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColGrupoUsuarioEdicao)
                .WithOne(gu => gu.UsuarioEdicao)
                .HasForeignKey(gu => gu.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColEmpresaCadastro)
                .WithOne(e => e.UsuarioCadastro)
                .HasForeignKey(e => e.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColEmpresaEdicao)
                .WithOne(e => e.UsuarioEdicao)
                .HasForeignKey(e => e.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColUsuarioCadastro)
                .WithOne(uc => uc.UsuarioCadastro)
                .HasForeignKey(uc => uc.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasMany(u => u.ColUsuarioEdicao)
                .WithOne(ue => ue.UsuarioEdicao)
                .HasForeignKey(ue => ue.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<UsuarioCliente>()
                .HasKey(ms => ms.ID);

            modelBuilder.Entity<UsuarioCliente>()
                .HasMany(u => u.ColUsuarioCadastro)
                .WithOne(uc => uc.UsuarioCadastro)
                .HasForeignKey(uc => uc.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<UsuarioCliente>()
                .HasMany(u => u.ColUsuarioEdicao)
                .WithOne(ue => ue.UsuarioEdicao)
                .HasForeignKey(ue => ue.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            #endregion

            #region Empresa

            modelBuilder.Entity<Company>()
                .HasMany(eu => eu.ColUsuario)
                .WithOne(s => s.Empresa)
                .HasForeignKey(s => s.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Company>()
                .HasMany(eu => eu.ColProduto)
                .WithOne(s => s.Empresa)
                .HasForeignKey(s => s.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Company>()
                .HasMany(eu => eu.ColCategoria)
                .WithOne(s => s.Empresa)
                .HasForeignKey(s => s.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Company>()
                .HasMany(eu => eu.ColQrCodeLayout)
                .WithOne(s => s.Empresa)
                .HasForeignKey(s => s.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            #endregion

            #region Produto

            modelBuilder.Entity<Product>()
                .HasMany(eu => eu.ColProdutoImagem)
                .WithOne(s => s.Produto)
                .HasForeignKey(s => s.ProdutoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            #endregion

            #region Grupo

            modelBuilder.Entity<Group>()
                .HasMany(g => g.ColEmpresa)
                .WithOne(e => e.Grupo)
                .HasForeignKey(e => e.GrupoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            #endregion

            #region GrupoUsuario

            modelBuilder.Entity<UserGroup>()
                .HasOne(gu => gu.Grupo)
                .WithMany(g => g.ColGrupoUsuario)
                .HasForeignKey(gu => gu.GrupoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<UserGroup>()
                .HasOne(gu => gu.Usuario)
                .WithMany(u => u.ColGrupoUsuario)
                .HasForeignKey(gu => gu.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            #endregion

            modelBuilder.Entity<Group>()
               .HasMany(g => g.ColSolicitacao)
               .WithOne(s => s.Grupo)
               .HasForeignKey(s => s.GrupoID)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

            modelBuilder.Entity<UsuarioEmpresa>()
            .HasKey(ue => ue.ID);

            modelBuilder.Entity<UsuarioEmpresa>()
                .HasOne(ue => ue.Empresa)
                .WithMany(e => e.ColUsuarioEmpresa)
                .HasForeignKey(ue => ue.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<UsuarioEmpresa>()
                .HasOne(ue => ue.Usuario)
                .WithMany(u => u.ColUsuarioEmpresa)
                .HasForeignKey(ue => ue.UsuarioID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Company>()
            .HasMany(e => e.ColBanner)
            .WithOne(b => b.Empresa)
            .HasForeignKey(b => b.EmpresaID)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

            modelBuilder.Entity<Company>()
                .HasMany(e => e.ColLogo)
                .WithOne(l => l.Empresa)
                .HasForeignKey(l => l.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<GrupoAdicionalItem>()
                .HasOne(pga => pga.GrupoAdicional)
                .WithMany(ga => ga.Produtos)
                .HasForeignKey(pga => pga.GrupoAdicionalID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<GrupoAdicionalItem>()
                .HasOne(pga => pga.Empresa)
                .WithMany(c => c.ColGrupoAdicionalItem)
                .HasForeignKey(pga => pga.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<GrupoAdicionalItem>()
                .HasOne(pga => pga.UsuarioCadastro)
                .WithMany(u => u.ColGrupoAdicionalItemCadastro)
                .HasForeignKey(pga => pga.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<GrupoAdicionalItem>()
                .HasOne(pga => pga.UsuarioEdicao)
                .WithMany(u => u.ColGrupoAdicionalItemEdicao)
                .HasForeignKey(pga => pga.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<GrupoAdicional>()
                .HasOne(ga => ga.Empresa)
                .WithMany(c => c.ColGrupoAdicional)
                .HasForeignKey(ga => ga.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<GrupoAdicional>()
                .HasOne(ga => ga.UsuarioCadastro)
                .WithMany(u => u.ColGrupoAdicionalCadastro)
                .HasForeignKey(ga => ga.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<GrupoAdicional>()
                .HasOne(ga => ga.UsuarioEdicao)
                .WithMany(u => u.ColGrupoAdicionalEdicao)
                .HasForeignKey(ga => ga.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<GrupoAdicional>()
                .HasMany(ga => ga.Produtos)
                .WithOne(pga => pga.GrupoAdicional)
                .HasForeignKey(pga => pga.GrupoAdicionalID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<GrupoAdicional>()
                .HasOne(ga => ga.Tipo)
                .WithMany(t => t.ColGrupoAdicional)
                .HasForeignKey(ga => ga.TipoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<ProdutoGrupoAdicional>()
                .HasKey(pg => new { pg.ProdutoID, pg.GrupoAdicionalID });

            modelBuilder.Entity<ProdutoGrupoAdicional>()
                .HasOne(pg => pg.Produto)
                .WithMany(p => p.ProdutoGruposAdicional)
                .HasForeignKey(pg => pg.ProdutoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<ProdutoGrupoAdicional>()
                .HasOne(pg => pg.GrupoAdicional)
                .WithMany(ga => ga.ProdutoGruposAdicional)
                .HasForeignKey(pg => pg.GrupoAdicionalID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<MesaStatus>()
                .HasKey(ms => ms.ID);

            modelBuilder.Entity<MesaStatus>()
                .Property(ms => ms.State)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<Mesa>()
                .HasOne(m => m.MesaStatus)
                .WithMany()
                .HasForeignKey(m => m.MesaStatusID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Mesa>()
                .HasKey(m => m.ID);

            modelBuilder.Entity<Mesa>()
                .Property(m => m.NomeMesa)
                .IsRequired();

            modelBuilder.Entity<Mesa>()
                .Property(m => m.Ativo)
                .HasDefaultValue(true);

            modelBuilder.Entity<Mesa>()
                .Property(m => m.DataCadastro)
                .HasColumnType("datetime2(0)")
                .IsRequired();

            modelBuilder.Entity<Mesa>()
                .Property(m => m.DataEdicao)
                .HasColumnType("datetime2(0)");

            modelBuilder.Entity<Mesa>()
                .HasOne(m => m.Empresa)
                .WithMany()
                .HasForeignKey(m => m.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Mesa>()
                .HasOne(m => m.UsuarioCadastro)
                .WithMany()
                .HasForeignKey(m => m.UsuarioIDCadastro)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Mesa>()
                .HasOne(m => m.UsuarioEdicao)
                .WithMany()
                .HasForeignKey(m => m.UsuarioIDEdicao)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QrCodeLayout>()
                .HasKey(ms => ms.ID);

            modelBuilder.Entity<CodigoVerificacao>()
                .HasKey(c => c.ID);

            #region Pedido

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Empresa)
                .WithMany(e => e.ColPedido)
                .HasForeignKey(p => p.EmpresaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.UsuarioCliente)
                .WithMany(u => u.ColPedido)
                .HasForeignKey(p => p.UsuarioClienteID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Pedido>()
                .HasOne(m => m.PedidoStatus)
                .WithMany()
                .HasForeignKey(m => m.StatusPedidoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Mesa)
                .WithMany(m => m.ColPedido)
                .HasForeignKey(p => p.MesaID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            #endregion

            #region PedidoItem

            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.Pedido)
                .WithMany(p => p.ColPedidoItem)
                .HasForeignKey(pi => pi.PedidoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<PedidoItem>()
                .HasOne(pi => pi.Produto)
                .WithMany()
                .HasForeignKey(pi => pi.ProdutoID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            #endregion

            #region PedidoItemAdicional

            modelBuilder.Entity<PedidoItemAdicional>()
                .HasOne(pia => pia.PedidoItem)
                .WithMany(pi => pi.ColPedidoItemAdicional)
                .HasForeignKey(pia => pia.PedidoItemID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<PedidoItemAdicional>()
                .HasOne(pia => pia.GrupoAdicionalItem)
                .WithMany()
                .HasForeignKey(pia => pia.GrupoAdicionalItemID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            #endregion

            #region PedidoStatus

            modelBuilder.Entity<PedidoStatus>()
                .HasKey(ps => ps.ID);

            #endregion

        }

        /// <summary>
        /// Método auxiliar para converter string para TimeSpan de forma robusta
        /// </summary>
        private static TimeSpan TryParseTimeSpan(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return TimeSpan.Zero;

            try
            {
                // Limpar a string removendo espaços e caracteres inválidos
                timeString = timeString.Trim();
                
                // Tentar diferentes formatos mais comuns
                if (TimeSpan.TryParseExact(timeString, @"hh\:mm\:ss", null, out TimeSpan result1))
                    return result1;
                
                if (TimeSpan.TryParseExact(timeString, @"h\:mm\:ss", null, out TimeSpan result2))
                    return result2;
                    
                if (TimeSpan.TryParseExact(timeString, @"hh\:mm", null, out TimeSpan result3))
                    return result3;
                    
                if (TimeSpan.TryParseExact(timeString, @"h\:mm", null, out TimeSpan result4))
                    return result4;
                
                // Tentar formato com segundos zerados "HH:mm:00"
                if (timeString.Length == 5 && timeString.Contains(':'))
                {
                    timeString += ":00";
                    if (TimeSpan.TryParseExact(timeString, @"hh\:mm\:ss", null, out TimeSpan result5))
                        return result5;
                }
                
                // Último recurso: TryParse genérico
                if (TimeSpan.TryParse(timeString, out TimeSpan result6))
                    return result6;
            }
            catch (Exception ex)
            {
                // Log do erro para debug
                Console.WriteLine($"[TimeSpan Convert Error] Failed to parse '{timeString}': {ex.Message}");
            }

            // Se nada funcionou, retorna zero
            Console.WriteLine($"[TimeSpan Convert Warning] Could not parse '{timeString}', using TimeSpan.Zero");
            return TimeSpan.Zero;
        }

        public DbSet<EnderecoDto> Enderecos { get; set; }

        public async Task<List<EnderecoDto>> ExecutarSPEnderecoAsync(string cep)
        {
            return await Set<EnderecoDto>().FromSqlRaw("EXEC [dbo].[SPEndereco] @Cep = {0}", cep).ToListAsync();
        }

        public DbSet<Product> Produto { get; set; }
        public DbSet<SolicitacaoDemonstracao> SolicitacaoDemonstracao { get; set; }
        public DbSet<UsuarioEmpresa> UsuarioEmpresa { get; set; }
        public DbSet<User> Usuario { get; set; }
        public DbSet<Company> Empresa { get; set; }
        public DbSet<UsuarioTipo> UsuarioTipo { get; set; }
        public DbSet<UsuarioStatus> UsuarioStatus { get; set; }
        public DbSet<Category> Categoria { get; set; }
        public DbSet<State> Estado { get; set; }
        public DbSet<City> Cidade { get; set; }
        public DbSet<ImageProduct> ProdutoImagem { get; set; }
        public DbSet<ThumbnailProduct> ProdutoThumbnail { get; set; }
        public DbSet<Layout> Layout { get; set; }
        public DbSet<Banner> Banner { get; set; }
        public DbSet<Logo> Logo { get; set; }
        public DbSet<Group> Grupo { get; set; }
        public DbSet<UserGroup> GrupoUsuario { get; set; }
        public DbSet<GrupoAdicionalItem> GrupoAdicionalItem { get; set; }
        public DbSet<GrupoAdicionalItemImagem> GrupoAdicionalItemImagem { get; set; }
        public DbSet<GrupoAdicional> GrupoAdicional { get; set; }
        public DbSet<TipoGrupoAdicional> TipoGrupoAdicional { get; set; }
        public DbSet<ProdutoGrupoAdicional> ProdutoGrupoAdicional { get; set; }
        public DbSet<Mesa> Mesa { get; set; }
        public DbSet<QrCodeLayout> QrCodeLayout { get; set; }
        public DbSet<MesaStatus> MesaStatus { get; set; }
        public DbSet<UsuarioCliente> UsuarioCliente { get; set; }
        public DbSet<CodigoVerificacao> CodigoVerificacao { get; set; }
        public DbSet<Pedido> Pedido { get; set; }
        public DbSet<PedidoItem> PedidoItem { get; set; }
        public DbSet<PedidoItemAdicional> PedidoItemAdicional { get; set; }
        public DbSet<ProdutoPromocaoHorario> ProdutoPromocaoHorarios { get; set; }
        public DbSet<PedidoStatus> PedidoStatus { get; set; }
        public DbSet<ProdutoHorario> ProdutoHorarios { get; set; }
        public DbSet<Promocao> Promocoes { get; set; }
        public DbSet<PromocaoHorario> PromocaoHorarios { get; set; }
        public DbSet<ProdutoPromocao> ProdutoPromocoes { get; set; }

    }
}
