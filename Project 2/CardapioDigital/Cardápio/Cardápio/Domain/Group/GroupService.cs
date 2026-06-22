using Cardápio.Client.Infra.Crypto;
using Cardápio.Domain.QrCode;
using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cardápio.Domain.Enterprise;
using Cardápio.Client.Pages.Empresas;



namespace Cardápio.Domain.Group
{
    public class GroupService
    {
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;
        private readonly QrCodeService _qrCodeService;
        private readonly Crypto _crypto;

        public GroupService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext,
                            Validator validator, QrCodeService qrCodeService, Crypto crypto)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
            _qrCodeService = qrCodeService;
            _crypto = crypto;
        }

        public async Task<ActionResult> GetGroups(ClaimsPrincipal User)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetUserByIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            _validator.VerifyUserIsMaster(usuario.UsuarioTipoID);

            IEnumerable<GrupoGetDTO> groups = await _cardapioUnitOfWork.GrupoRepo.GetAllGrupoAsync();

            return new OkObjectResult(groups);
        }

        public async Task<ActionResult> CreateGroup(ClaimsPrincipal User, GrupoAddDTO grupoInfos)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            UsuarioGetDTO usuario = await GetUserByIDAsync(UserID);
            _validator.VerifyUserIDIsPresent(usuario.EmpresaID);

            _validator.VerifyUserIsMaster(usuario.UsuarioTipoID);

            Infra.Model.Group grupoModel = new Infra.Model.Group
            {
                Nome = grupoInfos.Nome,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
                GrupoTipoID = grupoInfos.EmpresaTipoID,
                Excluido = false,
                Ativo = grupoInfos.Ativo,
            };

            await _cardapioUnitOfWork.GrupoRepo.AddAsync(grupoModel);

            await _cardapioUnitOfWork.Commit();

            var empresaModel = new Company
            {
                Nome = grupoInfos.NomeEmpresa,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                EstadoID = "PR",
                CidadeID = 6862,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
                Ativo = grupoInfos.Ativo,
                Excluido = false,
                DiasDemo = 10,
                EmpresaTipoID = grupoInfos.EmpresaTipoID,
                GrupoID = grupoModel.ID
            };

            await _cardapioUnitOfWork.EmpresaRepo.AddAsync(empresaModel);
            await _cardapioUnitOfWork.Commit();

            empresaModel.QRCode = await _qrCodeService.SaveQrCode(empresaModel.Nome);

            

            Infra.Model.User usuarioModel = new Infra.Model.User
            {
                Nome = grupoInfos.NomeUsuarioAdmin,
                Password = grupoInfos.UsuarioSenha,
                Email = grupoInfos.UsuarioLogin,
                EmpresaID = empresaModel.ID,
                GrupoID = grupoModel.ID,
                Excluido = false,
                UsuarioTipoID = 1,
                UsuarioStatusID = grupoInfos.EmpresaTipoID, // <-- direto do DTO, sem buscar no banco
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = DateTime.UtcNow,
                DataEdicao = DateTime.UtcNow,
                Ativo = grupoInfos.Ativo,
            };

            await _cardapioUnitOfWork.UsuarioRepo.AddAsync(usuarioModel);
            await _cardapioUnitOfWork.Commit();

            var usuarioGrupoModel = new UserGroup
            {
                GrupoID = grupoModel.ID,
                IsAdmin = true,
                UsuarioID = usuarioModel.ID,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
                Ativo = grupoInfos.Ativo,
            };

            await _cardapioUnitOfWork.GrupoUsuarioRepo.AddAsync(usuarioGrupoModel);
            await _cardapioUnitOfWork.Commit();

            var usuarioEmpresaModel = new UsuarioEmpresa
            {
                EmpresaID = empresaModel.ID,
                UsuarioID = usuarioModel.ID,
            };

            await _cardapioUnitOfWork.UsuarioEmpresaRepo.AddAsync(usuarioEmpresaModel);
            await _cardapioUnitOfWork.Commit();

            var logoModel = new Logo
            {
                Nome = "logo_default",
                Arquivo = "logo_default.png",
                Altura = 80,
                Largura = 80,
                Ativo = true,
                EmpresaID = empresaModel.ID,
                Tamanho = 0,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
            };

            await _cardapioUnitOfWork.LogoRepo.AddAsync(logoModel);
            await _cardapioUnitOfWork.Commit();

            var bannerModel = new Banner
            {
                Nome = "banner_default",
                Arquivo = "banner_default.png",
                Altura = 1300,
                Largura = 600,
                Ativo = true,
                EmpresaID = empresaModel.ID,
                Tamanho = 0,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
            };

            await _cardapioUnitOfWork.BannerRepo.AddAsync(bannerModel);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> UpdateGroup(ClaimsPrincipal User, GrupoUpdateDTO grupoInfos, int grupoID)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            UsuarioGetDTO usuario = await GetUserByIDAsync(UserID);
            _validator.VerifyUserIsMaster(usuario.UsuarioTipoID);

            Infra.Model.Group grupoExistente = await _cardapioUnitOfWork.GrupoRepo.GetGrupoModelByIDAsync(grupoID);

            grupoExistente.Nome = grupoInfos.Nome;
            grupoExistente.Ativo = grupoInfos.Ativo;
            grupoExistente.GrupoTipoID = grupoInfos.GrupoTipoID;

            if (grupoInfos.Ativo)
                grupoExistente.Excluido = false;

            // Busca todas as empresas do grupo e atualiza o EmpresaTipoID
            var listaEmpresas = await _cardapioUnitOfWork.EmpresaRepo.GetEnterprisesModelByGrupoID(grupoExistente.ID);

            if (listaEmpresas != null && listaEmpresas.Any())
            {
                foreach (var empresa in listaEmpresas)
                {
                    empresa.EmpresaTipoID = grupoInfos.GrupoTipoID; // GrupoTipoID 1=Demo, 2=Cliente, 3=Excluído
                    await _cardapioUnitOfWork.EmpresaRepo.UpdateAsync(empresa);
                }
            }

            await _cardapioUnitOfWork.GrupoRepo.UpdateAsync(grupoExistente);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }



        public async Task<ActionResult> ReturnGroup(ClaimsPrincipal User, int grupoID, int? empresaID = null)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            UsuarioGetDTO usuario = await GetUserByIDAsync(UserID);
            _validator.VerifyUserIsMaster(usuario.UsuarioTipoID);

            Infra.Model.Group grupoExistente = await _cardapioUnitOfWork.GrupoRepo.GetGrupoModelByIDAsync(grupoID);
            if (grupoExistente == null)
                throw new ErrorResponse($"Grupo {grupoID} não encontrado.", 404);

            IEnumerable<Company> companies = await _cardapioUnitOfWork.EmpresaRepo.GetEnterprisesModelByGrupoID(grupoExistente.ID);

            // Se passou empresaID, reativa só aquela empresa
            // Se não passou, reativa todas (retorno do grupo inteiro)
            var empresasParaReativar = empresaID.HasValue
                ? companies.Where(c => c.ID == empresaID.Value)
                : companies;

            foreach (var company in empresasParaReativar)
            {
                company.Excluido = false;
                company.Ativo = true;

                IEnumerable<Infra.Model.User> users = await _cardapioUnitOfWork.UsuarioRepo.GetAllUserByCompanyID(company.ID);
                foreach (Infra.Model.User user in users)
                {
                    user.Excluido = false;
                    user.Ativo = true;
                    await _cardapioUnitOfWork.UsuarioRepo.UpdateAsync(user);
                }

                await _cardapioUnitOfWork.EmpresaRepo.UpdateAsync(company);
            }

            // Só reativa o grupo se não for retorno de empresa específica
            if (!empresaID.HasValue)
            {
                grupoExistente.Excluido = false;
                grupoExistente.Ativo = true;
                await _cardapioUnitOfWork.GrupoRepo.UpdateAsync(grupoExistente);
            }

            await _cardapioUnitOfWork.Commit();
            return new OkResult();
        }

        public async Task<ActionResult> DeleteGroup(ClaimsPrincipal User, int grupoID)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            UsuarioGetDTO usuario = await GetUserByIDAsync(UserID);
            _validator.VerifyUserIsMaster(usuario.UsuarioTipoID);

            Infra.Model.Group grupoExistente = await _cardapioUnitOfWork.GrupoRepo.GetGrupoModelByIDAsync(grupoID);
            IEnumerable<Company> companies = await _cardapioUnitOfWork.EmpresaRepo.GetEnterprisesModelByGrupoID(grupoExistente.ID);
            foreach (var company in companies)
            {
                company.Excluido = true;
                company.Ativo = false;

                IEnumerable<Infra.Model.User> users = await _cardapioUnitOfWork.UsuarioRepo.GetAllUserByCompanyID(company.ID);

                foreach (Infra.Model.User user in users)
                {
                    user.Excluido = true;
                    user.Ativo = false;
                }
            }

            grupoExistente.Excluido = true;
            grupoExistente.Ativo = false;

            await _cardapioUnitOfWork.GrupoRepo.UpdateAsync(grupoExistente);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        private async Task<UsuarioGetDTO> GetUserByIDAsync(int usuarioID)
        {
            UsuarioGetDTO user = await _cardapioUnitOfWork.UsuarioRepo.GetUsuarioByIDAsync(usuarioID);

            return user;
        }
    }
}
