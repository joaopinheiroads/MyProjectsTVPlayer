using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Infra.Repositories
{
    public class GrupoRepo : IGrupoRepo
    {
        private readonly AppDbContext context;

        public GrupoRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(Group grupo)
        {
            try
            {
                if (grupo == null)
                {
                    throw new ArgumentNullException(nameof(grupo));
                }
                await context.Grupo.AddAsync(grupo);
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(Group grupo)
        {
            grupo.DataEdicao = DateTime.Now;
            await Task.FromResult(0);
        }

        public async Task<Group> GetByIDAsync(int grupoID, int empresaID)
        {
            return await (from grupo in context.Grupo
                          where grupo.ID == grupoID &&
                                grupo.Ativo == true
                          select grupo
                        ).FirstOrDefaultAsync();
        }

        public async Task<GrupoGetDTO> GetGrupoByIDAsync(int grupoID)
        {
            return await (from grupo in context.Grupo
                          where grupo.ID == grupoID &&
                                grupo.Ativo == true
                          select new GrupoGetDTO
                          {
                              ID = grupo.ID,
                              Nome = grupo.Nome,
                              GrupoTipoID = grupo.GrupoTipoID ?? 0
                          }).FirstOrDefaultAsync();
        }

        public async Task<Group> GetGrupoModelByIDAsync(int grupoID)
        {
            return await (from grupo in context.Grupo
                          where grupo.ID == grupoID
                          select grupo).FirstOrDefaultAsync();
        }

        public async Task<List<GrupoGetDTO>> GetGrupoByUsuarioIDAsync(int usuarioID)
        {
            var grupos = await context.Grupo
                .Where(grupo => grupo.ColUsers.Any(user => user.ID == usuarioID) && grupo.Ativo)
                .Select(grupo => new GrupoGetDTO
                {
                    ID = grupo.ID,
                    Nome = grupo.Nome,
                })
                .ToListAsync();

            return grupos;
        }

        public async Task<IEnumerable<GrupoGetDTO>> GetAllGrupoAsync()
        {
            var grupos = await context.Grupo
                .Where(grupo => grupo.ID != 1)
                .ToListAsync();

            var grupoGetDTOs = new List<GrupoGetDTO>();

            foreach (Group grupo in grupos)
            {
                var empresas = await GetEnterprisesByUsuarioIDAsync(grupo.ID);

                grupoGetDTOs.Add(new GrupoGetDTO
                {
                    ID = grupo.ID,
                    Nome = grupo.Nome,
                    Empresas = empresas,
                    GrupoTipoID = grupo.GrupoTipoID ?? 1,
                    Ativo = grupo.Ativo,
                    Excluido = grupo.Excluido
                });
            }

            return grupoGetDTOs;
        }

        public async Task<IEnumerable<EmpresaGetDTO>> GetEnterprisesByUsuarioIDAsync(int grupoID)
        {
            var empresas = await (from usuarioEmpresa in context.UsuarioEmpresa
                                  join empresa in context.Empresa on usuarioEmpresa.EmpresaID equals empresa.ID
                                  join estado in context.Estado on usuarioEmpresa.Empresa.EstadoID equals estado.EstID
                                  join logo in context.Logo on usuarioEmpresa.EmpresaID equals logo.EmpresaID into logoGroup
                                  from logo in logoGroup.DefaultIfEmpty()
                                  join banner in context.Banner on usuarioEmpresa.EmpresaID equals banner.EmpresaID into bannerGroup
                                  from banner in bannerGroup.DefaultIfEmpty()
                                  join cidade in context.Cidade on usuarioEmpresa.Empresa.CidadeID equals cidade.CidID
                                  where empresa.GrupoID == grupoID
                                  select new EmpresaGetDTO
                                  {
                                      ID = usuarioEmpresa.EmpresaID,
                                      Nome = usuarioEmpresa.Empresa.Nome ?? "Não informado",
                                      Celular = usuarioEmpresa.Empresa.Celular ?? "Não informado",
                                      CEP = usuarioEmpresa.Empresa.CEP ?? "Não informado",
                                      CidadeID = usuarioEmpresa.Empresa.CidadeID ?? 0,
                                      CNPJ = usuarioEmpresa.Empresa.CNPJ ?? "Não informado",
                                      EmpresaTipoID = usuarioEmpresa.Empresa.EmpresaTipoID ?? EmpresaTipoHelper.Demo,
                                      EstadoID = usuarioEmpresa.Empresa.EstadoID,
                                      QRCode = usuarioEmpresa.Empresa.QRCode ?? "Não informado",
                                      RazaoSocial = usuarioEmpresa.Empresa.RazaoSocial ?? "Não informado",
                                      Telefone = usuarioEmpresa.Empresa.Telefone ?? "Não informado",
                                      ImageLogo = logo.Arquivo ?? "Não informado",
                                      ImageBanner = banner.Arquivo ?? "Não informado",
                                      Ativo = usuarioEmpresa.Empresa.Ativo,
                                      Excluido = usuarioEmpresa.Empresa.Excluido,
                                      DataCadastro = usuarioEmpresa.Empresa.DataCadastro,
                                      DiasDemo = usuarioEmpresa.Empresa.DiasDemo,
                                      Usuarios = (from ue in context.UsuarioEmpresa
                                                  join u in context.Usuario on ue.UsuarioID equals u.ID
                                                  where ue.EmpresaID == empresa.ID
                                                  select new UsuarioGetDTO
                                                  {
                                                      ID = u.ID,
                                                      Nome = u.Nome,
                                                      Ativo = u.Ativo,
                                                      Email = u.Email,
                                                      EmpresaNome = u.Empresa.Nome,
                                                      GrupoNome = u.Grupo.Nome,
                                                      EmpresaID = u.EmpresaID,
                                                      GrupoID = u.GrupoID,
                                                      UsuarioTipoID = u.UsuarioTipoID
                                                  }).ToList(),
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
                                  }).ToListAsync();

            foreach (var empresa in empresas)
            {
                empresa.Usuarios = empresa.Usuarios.Distinct().ToList();
            }

            empresas = empresas.DistinctBy(e => e.ID).ToList();

            return empresas;
        }
    }
}
