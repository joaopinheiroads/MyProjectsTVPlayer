using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;

namespace Cardápio.Infra.Repositories
{
    public class EmpresaRepo : IEmpresaRepo
    {
        private readonly AppDbContext context;

        public EmpresaRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<int> GetEmpresaIDAsync(string qrCode)
        {
            return await context.Empresa.Where(empresa => empresa.QRCode == qrCode).Select(empresa => empresa.ID).FirstOrDefaultAsync();
        }

        public async Task<Company> GetByIDAsync(int empresaID)
        {
            return await context.Empresa.Where(empresa => empresa.ID == empresaID).FirstOrDefaultAsync();
        }

        public async Task<Company> GetByIDAsync(int empresaID, int ID)
        {
            return await context.Empresa.Where(empresa => empresa.ID == empresaID).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EmpresaGetDTO>> GetEnterpriseByGrupoID(int grupoID)
        {
            return await (from usuarioEmpresa in context.UsuarioEmpresa
                          join grupoUsuario in context.GrupoUsuario on usuarioEmpresa.UsuarioID equals grupoUsuario.UsuarioID
                          where grupoUsuario.GrupoID == grupoID && usuarioEmpresa.Empresa.Ativo
                          select new EmpresaGetDTO
                          {
                              ID = usuarioEmpresa.EmpresaID,
                              Nome = usuarioEmpresa.Empresa.Nome,
                              Celular = usuarioEmpresa.Empresa.Celular,
                              CEP = usuarioEmpresa.Empresa.CEP,
                              CidadeID = usuarioEmpresa.Empresa.CidadeID ?? 0,
                              CNPJ = usuarioEmpresa.Empresa.CNPJ,
                              EstadoID = usuarioEmpresa.Empresa.EstadoID,
                              QRCode = usuarioEmpresa.Empresa.QRCode,
                              RazaoSocial = usuarioEmpresa.Empresa.RazaoSocial,
                              Telefone = usuarioEmpresa.Empresa.Telefone
                          }).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<Company>> GetEnterprisesModelByGrupoID(int grupoID)
        {
            return await (from usuarioEmpresa in context.UsuarioEmpresa
                          join grupoUsuario in context.GrupoUsuario on usuarioEmpresa.UsuarioID equals grupoUsuario.UsuarioID
                          where grupoUsuario.GrupoID == grupoID
                          select usuarioEmpresa.Empresa).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<EmpresaGetDTO>> GetEnterprisesByUsuarioIDAsync(int usuarioID, int grupoID, int usuarioTipoID, int empresaID)
        {
            return await (from usuarioEmpresa in context.UsuarioEmpresa
                          join usuario in context.Usuario on usuarioEmpresa.UsuarioID equals usuario.ID
                          join grupoUsuario in context.GrupoUsuario on usuario.ID equals grupoUsuario.UsuarioID into grupoUsuarioGroup
                          from grupoUsuario in grupoUsuarioGroup.DefaultIfEmpty()
                          join estado in context.Estado on usuarioEmpresa.Empresa.EstadoID equals estado.EstID
                          join logo in context.Logo on usuarioEmpresa.EmpresaID equals logo.EmpresaID into logoGroup
                          from logo in logoGroup.DefaultIfEmpty()
                          join banner in context.Banner on usuarioEmpresa.EmpresaID equals banner.EmpresaID into bannerGroup
                          from banner in bannerGroup.DefaultIfEmpty()
                          join cidade in context.Cidade on usuarioEmpresa.Empresa.CidadeID equals cidade.CidID
                          where usuarioEmpresa.Empresa.Ativo == true &&
                           usuarioEmpresa.Empresa.Excluido == false &&
                           grupoUsuario != null &&
                           grupoUsuario.GrupoID == grupoID &&
                           usuarioEmpresa.UsuarioID == usuarioID &&
                           usuario.UsuarioTipoID != 4
                          select new EmpresaGetDTO
                          {
                              ID = usuarioEmpresa.EmpresaID,
                              Nome = usuarioEmpresa.Empresa.Nome,
                              Celular = usuarioEmpresa.Empresa.Celular,
                              CEP = usuarioEmpresa.Empresa.CEP,
                              CidadeID = usuarioEmpresa.Empresa.CidadeID ?? 0,
                              CNPJ = usuarioEmpresa.Empresa.CNPJ,
                              EstadoID = usuarioEmpresa.Empresa.EstadoID,
                              QRCode = usuarioEmpresa.Empresa.QRCode,
                              RazaoSocial = usuarioEmpresa.Empresa.RazaoSocial,
                              Telefone = usuarioEmpresa.Empresa.Telefone,
                              ImageLogo = logo.Arquivo,
                              ImageBanner = banner.Arquivo,
                              Ativo = usuarioEmpresa.Empresa.Ativo,
                              Excluido = usuarioEmpresa.Empresa.Excluido,
                              AtenderWhatsapp = usuarioEmpresa.Empresa.AtenderWhatsapp,
                              Estado = new EstadoGetDTO
                              {
                                  EndEstadoID = estado.EstID,
                                  EstNome = estado.EstNome
                              },
                              Cidade = new CidadeGetDTO
                              {
                                  CidID = cidade.CidID,
                                  CidNome = cidade.CidNome
                              }
                          }).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<EmpresaSelectDTO>> GetEmpresaByUsuarioIDSelectAsync(int usuarioID, int usuarioTipoID)
        {
            return await (from empresa in context.Empresa
                          where empresa.Ativo == true
                          select new EmpresaSelectDTO
                          {
                              ID = empresa.ID,
                              Nome = empresa.Nome,
                          }).ToListAsync();
        }

        public async Task<IEnumerable<EmpresaGetDTO>> GetEmpresaByUsuarioIDAsync(int usuarioID, int usuarioTipoID)
        {
            return await (from empresa in context.Empresa
                          where empresa.Ativo == true
                          select new EmpresaGetDTO
                          {
                              ID = empresa.ID,
                              Nome = empresa.Nome,
                              QRCode = empresa.QRCode,
                              RazaoSocial = empresa.RazaoSocial,
                              CNPJ = empresa.CNPJ,
                              Celular = empresa.Celular,
                              Telefone = empresa.Telefone,
                              CEP = empresa.CEP,
                              EstadoID = empresa.EstadoID,
                              CidadeID = empresa.CidadeID ?? 0,
                              AtenderWhatsapp = empresa.AtenderWhatsapp
                          }).ToListAsync();
        }

        public async Task<EmpresaGetDTO> GetEmpresaByNameAsync(string empresaNome)
        {
            string normalizedEmpresaNome = empresaNome.Replace("-", " ").Trim();

            return await (from empresa in context.Empresa
                          join logo in context.Logo on empresa.ID equals logo.EmpresaID into logoGroup
                          from logo in logoGroup.DefaultIfEmpty()
                          join banner in context.Banner on empresa.ID equals banner.EmpresaID into bannerGroup
                          from banner in bannerGroup.DefaultIfEmpty()
                          where empresa.Ativo == true &&
                                empresa.Nome.Replace("-", "").Replace(" ", "").ToLower() ==
                                normalizedEmpresaNome.Replace("-", "").Replace(" ", "").ToLower()
                          select new EmpresaGetDTO
                          {
                              ID = empresa.ID,
                              Nome = empresa.Nome,
                              QRCode = empresa.QRCode,
                              RazaoSocial = empresa.RazaoSocial,
                              ImageBanner = banner.Arquivo,
                              ImageLogo = logo.Arquivo,
                              CNPJ = empresa.CNPJ,
                              Celular = empresa.Celular,
                              Telefone = empresa.Telefone,
                              CEP = empresa.CEP,
                              Ativo = empresa.Ativo,
                              Excluido = empresa.Excluido,
                              EstadoID = empresa.EstadoID,
                              DiasDemo = empresa.DiasDemo,
                              AtenderWhatsapp = empresa.AtenderWhatsapp,
                              EmpresaTipoID = empresa.EmpresaTipoID ?? 0,
                              CidadeID = empresa.CidadeID ?? 0
                          }).FirstOrDefaultAsync();
        }

        public async Task<EmpresaGetDTO> GetEmpresaByIDAsync(int empresaID)
        {
            return await (from empresa in context.Empresa
                          join logo in context.Logo on empresa.ID equals logo.EmpresaID into logoGroup
                          from logo in logoGroup.DefaultIfEmpty()
                          join banner in context.Banner on empresa.ID equals banner.EmpresaID into bannerGroup
                          from banner in bannerGroup.DefaultIfEmpty()
                          where empresa.Ativo == true &&
                                  empresa.ID == empresaID
                          select new EmpresaGetDTO
                          {
                              ID = empresa.ID,
                              Nome = empresa.Nome,
                              QRCode = empresa.QRCode,
                              RazaoSocial = empresa.RazaoSocial,
                              ImageBanner = banner.Arquivo,
                              ImageLogo = logo.Arquivo,
                              CNPJ = empresa.CNPJ,
                              Celular = empresa.Celular,
                              Telefone = empresa.Telefone,
                              CEP = empresa.CEP,
                              EmpresaTipoID = empresa.EmpresaTipoID ?? 0,
                              Excluido = empresa.Excluido,
                              AtenderWhatsapp = empresa.AtenderWhatsapp,
                              EstadoID = empresa.EstadoID,
                              CidadeID = empresa.CidadeID ?? 0
                          }).FirstOrDefaultAsync();
        }


        public async Task<string> GetQRCodeByIDAsync(int empresaID)
        {
            string qrcode = await (from emp in context.Empresa
                                   where emp.ID == empresaID &&
                                        emp.Ativo == true
                                   select emp.QRCode
                                  ).FirstOrDefaultAsync();
            return qrcode;
        }

        public async Task AddAsync(Company entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.Empresa.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(Company empresa)
        {
            empresa.DataEdicao = DateTime.Now;

            await Task.FromResult(0);
        }
    }
}
