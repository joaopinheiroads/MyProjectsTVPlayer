using Microsoft.EntityFrameworkCore;
using Cardápio.Dto;
using Cardápio.Infra.Model;
using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Data;
using Microsoft.EntityFrameworkCore;


namespace Cardápio.Infra.Repositories
{
    public class UsuarioRepo : IUsuarioRepo
    {
        private readonly AppDbContext context;

        public UsuarioRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<User>> GetAllUserByCompanyIDNonAdmin(int empresaID)
        {
            return await (from usuario in context.Usuario
                          where usuario.EmpresaID == empresaID
                             && (usuario.UsuarioTipoID == 2
                                || usuario.UsuarioTipoID == 3)
                          select usuario).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUserByCompanyID(int empresaID)
        {
            return await (from usuario in context.Usuario
                          where usuario.EmpresaID == empresaID
                          select usuario).ToListAsync();
        }

        public async Task<IEnumerable<UsuarioGetDTO>> GetUsuarioAllAsync()
        {
            return await (from usuario in context.Usuario
                          join usuarioTipo in context.UsuarioTipo on usuario.UsuarioTipoID equals usuarioTipo.ID
                          join empresa in context.Empresa on usuario.EmpresaID equals empresa.ID
                          where usuario.Ativo == true &&
                                usuarioTipo.Ativo == true &&
                                empresa.Ativo == true
                          orderby empresa.Nome, usuario.Nome
                          select new UsuarioGetDTO
                          {
                              ID = usuario.ID,
                              Nome = usuario.Nome,
                              Email = usuario.Email,
                              Password = usuario.Password,
                              UsuarioTipoID = usuario.UsuarioTipoID,
                              UsuarioTipo = usuarioTipo.Nome,
                              EmpresaID = usuario.EmpresaID,
                              EmpresaNome = empresa.Nome
                          }).ToListAsync();
        }

        public async Task<User> GetUsuarioAdminByGrupoId(int groupID)
        {
            return await (from usuario in context.Usuario
                          where usuario.GrupoID == groupID
                             && usuario.UsuarioTipoID == 1
                          select usuario).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<UsuarioGetDTO>> GetAllUsuarioByGroupIDAsync(int groupID, int userID, bool isAdmin)
        {
            // Pega as empresas que o usuário logado está vinculado
            var empresasDoUsuario = context.UsuarioEmpresa
                .Where(ue => ue.UsuarioID == userID)
                .Select(ue => ue.EmpresaID);

            return await (from usuarioEmpresa in context.UsuarioEmpresa
                          join usuario in context.Usuario on usuarioEmpresa.UsuarioID equals usuario.ID
                          join grupoUsuario in context.GrupoUsuario on usuario.ID equals grupoUsuario.UsuarioID into grupoUsuarioGroup
                          from grupoUsuario in grupoUsuarioGroup.DefaultIfEmpty()
                          join empresa in context.Empresa on usuarioEmpresa.EmpresaID equals empresa.ID
                          where
                            usuario.Excluido == false &&
                            usuario.UsuarioTipoID != 4 &&
                            grupoUsuario != null &&
                            grupoUsuario.GrupoID == groupID &&
                            empresasDoUsuario.Contains(usuarioEmpresa.EmpresaID) // só empresas do usuário logado
                          select new UsuarioGetDTO
                          {
                              ID = usuario.ID,
                              Nome = usuario.Nome,
                              Email = usuario.Email,
                              Password = usuario.Password,
                              UsuarioTipo = usuario.UsuarioTipo.Nome,
                              UsuarioTipoID = usuario.UsuarioTipoID,
                              EmpresaNome = empresa.Nome,
                              GrupoNome = grupoUsuario.Grupo.Nome,
                              EmpresaID = usuarioEmpresa.EmpresaID,
                              GrupoID = usuario.GrupoID,
                              Ativo = usuario.Ativo
                          }).Distinct().ToListAsync();
        }

        public async Task<IEnumerable<UsuarioGetDTO>> GetUsuarioByUsuarioIDAsync(int usuarioID)
        {
            return await (from usuario in context.Usuario
                          join usuarioTipo in context.UsuarioTipo on usuario.UsuarioTipoID equals usuarioTipo.ID
                          join empresa in context.Empresa on usuario.EmpresaID equals empresa.ID
                          where usuario.Ativo == true &&
                                usuarioTipo.Ativo == true &&
                                empresa.Ativo == true
                          select new UsuarioGetDTO
                          {
                              ID = usuario.ID,
                              Nome = usuario.Nome,
                              Email = usuario.Email,
                              UsuarioTipoID = usuario.UsuarioTipoID,
                              UsuarioTipo = usuarioTipo.Nome,
                              EmpresaID = usuario.EmpresaID,
                              EmpresaNome = empresa.Nome
                          }).ToListAsync();
        }

        public async Task<IEnumerable<UsuarioGetDTO>> GetUsuarioByEmpresaIDAsync(int empresaID)
        {
            return await (from usuario in context.Usuario
                          join usuarioTipo in context.UsuarioTipo on usuario.UsuarioTipoID equals usuarioTipo.ID
                          join empresa in context.Empresa on usuario.EmpresaID equals empresa.ID
                          where usuario.Ativo == true &&
                                usuarioTipo.Ativo == true &&
                                empresa.Ativo == true &&
                                empresa.ID == empresaID
                          select new UsuarioGetDTO
                          {
                              ID = usuario.ID,
                              Nome = usuario.Nome,
                              Email = usuario.Email,
                              UsuarioTipoID = usuario.UsuarioTipoID,
                              UsuarioTipo = usuarioTipo.Nome,
                              EmpresaID = usuario.EmpresaID,
                              EmpresaNome = empresa.Nome
                          }).ToListAsync();
        }

        public async Task<UsuarioGetDTO> GetUsuarioByIDAsync(int usuarioID)
        {
            return await (from usuario in context.Usuario
                          join usuarioTipo in context.UsuarioTipo on usuario.UsuarioTipoID equals usuarioTipo.ID
                          join usuarioStatus in context.UsuarioStatus on usuario.UsuarioStatusID equals usuarioStatus.ID
                          join empresa in context.Empresa on usuario.EmpresaID equals empresa.ID
                          join grupo in context.Grupo on usuario.GrupoID equals grupo.ID into grupoJoin
                          from grupo in grupoJoin.DefaultIfEmpty()
                          where usuario.ID == usuarioID &&
                                usuario.Ativo == true &&
                                usuarioTipo.Ativo == true &&
                                empresa.Ativo == true &&
                                (grupo == null || grupo.Ativo == true)
                          select new UsuarioGetDTO
                          {
                              ID = usuario.ID,
                              Nome = usuario.Nome,
                              Email = usuario.Email,
                              UsuarioTipoID = usuario.UsuarioTipoID,
                              UsuarioStatusID = usuario.UsuarioStatusID,
                              UsuarioTipo = usuarioTipo.Nome,
                              EmpresaID = usuario.EmpresaID,
                              EmpresaNome = empresa.Nome,
                              Ativo = true,
                              GrupoID = grupo != null ? grupo.ID : null,
                          }).FirstOrDefaultAsync();
        }

        public async Task<UsuarioGetDTO> GetUsuarioAuthAsync(string email, string password)
        {
            return await (from usuario in context.Usuario
                          join usuarioTipo in context.UsuarioTipo on usuario.UsuarioTipoID equals usuarioTipo.ID
                          join empresa in context.Empresa on usuario.EmpresaID equals empresa.ID
                          where usuario.Email == email &&
                                usuario.Password == password &&
                                usuario.Ativo == true &&
                                usuarioTipo.Ativo == true &&
                                empresa.Ativo == true
                          select new UsuarioGetDTO
                          {
                              ID = usuario.ID,
                              Nome = usuario.Nome,
                              Email = usuario.Email,
                              UsuarioTipoID = usuario.UsuarioTipoID,
                              UsuarioStatusID = usuario.UsuarioStatusID,
                              UsuarioTipo = usuarioTipo.Nome,
                              EmpresaID = usuario.EmpresaID
                          }).FirstOrDefaultAsync();
        }

        public async Task AddAsync(User entity)
        {
            try
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }
                await context.Usuario.AddAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<User> GetByIDAsync(int usuarioID, int empresaID)
        {
            return await (from usuario in context.Usuario
                          where usuario.ID == usuarioID
                          select usuario).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(User entity)
        {
            entity.DataEdicao = DateTime.Now;

            await Task.FromResult(0);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
             return await (from usuario in context.Usuario
                          where usuario.Email == email
                          select usuario).FirstOrDefaultAsync();



        }
    }
}
