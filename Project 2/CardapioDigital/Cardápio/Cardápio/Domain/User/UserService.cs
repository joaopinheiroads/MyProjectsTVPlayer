using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using Cardápio.Domain;
using Cardápio.Domain.Codigo;



namespace Cardápio.Domain.User
{
    public class UserService
    {
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;

        private readonly DisparosChatService _disparosChat;
        
        private readonly CodigoVerificacaoService _codigoVerificacaoService;

        public UserService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext, Validator validator, DisparosChatService disparosChat, CodigoVerificacaoService codigoVerificacaoService)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
            _disparosChat = disparosChat;
            _codigoVerificacaoService = codigoVerificacaoService;
            

        }

        public async Task<ActionResult> CompletarSignup(ClaimsPrincipal UserClaims, CompletarCadastroAddDTO completarCadastroAddDTO)
        {
            int UserID = int.Parse(UserClaims.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            UsuarioCliente usuarioDB = await _cardapioUnitOfWork.UsuarioClienteRepo.GetByIDAsync(UserID);

            if (usuarioDB.Email == null)
            {
                usuarioDB.Email = completarCadastroAddDTO.Email;
            }

            if (usuarioDB.Celular == null)
            {
                usuarioDB.Celular = completarCadastroAddDTO.Celular;
            }

            usuarioDB.CPF = completarCadastroAddDTO.CPF;
            usuarioDB.Nome = completarCadastroAddDTO.Nome;

            await _cardapioUnitOfWork.UsuarioClienteRepo.UpdateAsync(usuarioDB);
            await _cardapioUnitOfWork.Commit();

            return new OkObjectResult(new());
        }

        public async Task<ActionResult> GetUsers(ClaimsPrincipal UserClaims)
        {
            int UserID = int.Parse(UserClaims.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            int grupoID = await GetGroupIDAsync(usuario.ID);
            UserGroup groupUser = await GetGroupUserIDAsync(UserID);

            IEnumerable<UsuarioGetDTO> usersList = await _cardapioUnitOfWork.UsuarioRepo.GetAllUsuarioByGroupIDAsync(usuario.GrupoID ?? 0, usuario.ID, groupUser.IsAdmin);

            return new OkObjectResult(usersList);
        }

        public async Task<ActionResult> ExcluirUsuario(ClaimsPrincipal UserClaims, int usuarioIDDelete)
        {
            int UserID = int.Parse(UserClaims.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(usuarioIDDelete);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            Infra.Model.User userToDelete = await _cardapioUnitOfWork.UsuarioRepo.GetByIDAsync(usuarioIDDelete, usuario.EmpresaID ?? 0);
            _validator.VerifyCategoryIdIsPresent(userToDelete.ID);

            userToDelete.Ativo = false;
            userToDelete.Excluido = true;
            userToDelete.UsuarioIDEdicao = UserID;

            await _cardapioUnitOfWork.UsuarioRepo.UpdateAsync(userToDelete);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> CriarUsuario(ClaimsPrincipal UserClaims, UsuarioAddDTO novoUsuario)
        {
            int UserID = int.Parse(UserClaims.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            int grupoID = await GetGroupIDAsync(usuario.ID);
            _validator.VerifyGroupIdIsPresent(usuario.EmpresaID);

            Infra.Model.User newUser = new Infra.Model.User
            {
                Nome = novoUsuario.Nome,
                Password = novoUsuario.Password,
                Email = novoUsuario.Email,
                DataCadastro = DateTime.Now,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                UsuarioTipoID = novoUsuario.UsuarioTipoID,
                UsuarioStatusID = usuario.UsuarioStatusID,
                GrupoID = grupoID,
                EmpresaID = novoUsuario.EmpresaID,
                Ativo = true
            };

            await _cardapioUnitOfWork.UsuarioRepo.AddAsync(newUser);
            await _cardapioUnitOfWork.Commit();

            UserGroup newUserGroup = new UserGroup
            {
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                GrupoID = grupoID,
                UsuarioID = newUser.ID,
                Ativo = true
            };

            if (novoUsuario.UsuarioTipoID == 3)
            {
                newUserGroup.IsAdmin = true;
            }

            await _cardapioUnitOfWork.GrupoUsuarioRepo.AddAsync(newUserGroup);

            UsuarioEmpresa newUserCompany = new UsuarioEmpresa
            {
                EmpresaID = novoUsuario.EmpresaID ?? 0,
                UsuarioID = newUser.ID
            };

            await _cardapioUnitOfWork.UsuarioEmpresaRepo.AddAsync(newUserCompany);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> CriarUsuarioInCompanyID(ClaimsPrincipal UserClaims, UsuarioAddDTO novoUsuario, int groupID)
        {
            int UserID = int.Parse(UserClaims.FindFirstValue(ClaimTypes.NameIdentifier));

            // Verifica se já existe usuário com esse e-mail no grupo
            Infra.Model.User usuarioExistente = await _cardapioUnitOfWork.UsuarioRepo.GetByEmailAsync(novoUsuario.Email);

            if (usuarioExistente != null)
            {
                // Só vincula à nova empresa
                UsuarioEmpresa novoVinculo = new UsuarioEmpresa
                {
                    EmpresaID = novoUsuario.EmpresaID ?? 0,
                    UsuarioID = usuarioExistente.ID
                };
                await _cardapioUnitOfWork.UsuarioEmpresaRepo.AddAsync(novoVinculo);
                await _cardapioUnitOfWork.Commit();
                return new OkResult();
            }

            // Cria normalmente se não existe
            GrupoGetDTO grupo = await GetGroupAsyncById(groupID);
            Infra.Model.User newUser = new Infra.Model.User
            {
                Nome = novoUsuario.Nome,
                Password = novoUsuario.Password,
                Email = novoUsuario.Email,
                DataCadastro = DateTime.Now,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                UsuarioTipoID = novoUsuario.UsuarioTipoID,
                UsuarioStatusID = grupo.GrupoTipoID,
                GrupoID = grupo.ID,
                EmpresaID = novoUsuario.EmpresaID,
                Ativo = true
            };

            await _cardapioUnitOfWork.UsuarioRepo.AddAsync(newUser);
            await _cardapioUnitOfWork.Commit();

            UserGroup newUserGroup = new UserGroup
            {
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                GrupoID = grupo.ID,
                UsuarioID = newUser.ID,
                Ativo = true,
                IsAdmin = novoUsuario.UsuarioTipoID == 1
            };

            await _cardapioUnitOfWork.GrupoUsuarioRepo.AddAsync(newUserGroup);

            UsuarioEmpresa newUserCompany = new UsuarioEmpresa
            {
                EmpresaID = novoUsuario.EmpresaID ?? 0,
                UsuarioID = newUser.ID
            };

            await _cardapioUnitOfWork.UsuarioEmpresaRepo.AddAsync(newUserCompany);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> EditarUsuario(ClaimsPrincipal UserClaims, UsuarioUpdateDTO usuarioEdit, int usuarioIDEdit)
        {
            int UserID = int.Parse(UserClaims.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            Infra.Model.User usuarioExistente = await _cardapioUnitOfWork.UsuarioRepo.GetByIDAsync(usuarioIDEdit, usuario.EmpresaID ?? 0);
            _validator.VerifyCategoryIdIsPresent(usuarioExistente.ID);

            usuarioExistente.Nome = usuarioEdit.Nome;
            usuarioExistente.Password = usuarioEdit.Password;
            usuarioExistente.Email = usuarioEdit.Email;
            usuarioExistente.UsuarioIDEdicao = UserID;
            usuarioExistente.UsuarioTipoID = usuarioEdit.UsuarioTipoID;
            usuarioExistente.UsuarioStatusID = usuario.UsuarioStatusID;
            usuarioExistente.Ativo = usuarioEdit.Ativo;

            UsuarioEmpresa usuarioEmpresa = await _cardapioUnitOfWork.UsuarioEmpresaRepo.GetByIDAsync(usuarioExistente.ID, usuarioExistente.EmpresaID ?? 0);
            if (usuarioEmpresa != null)
            {
                usuarioExistente.EmpresaID = usuarioEdit.EmpresaID;
                usuarioEmpresa.EmpresaID = usuarioExistente.EmpresaID ?? 0;
                await _cardapioUnitOfWork.UsuarioEmpresaRepo.UpdateAsync(usuarioEmpresa);
                await _cardapioUnitOfWork.UsuarioRepo.UpdateAsync(usuarioExistente);
            }

            UserGroup groupUser = await GetGroupUserIDAsync(UserID);

            if (usuarioEdit.UsuarioTipoID == 3)
            {
                groupUser.IsAdmin = true;
            }

            await _cardapioUnitOfWork.GrupoUsuarioRepo.UpdateAsync(groupUser);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        private async Task<GrupoGetDTO> GetGroupAsyncById(int groupID)
        {
            GrupoGetDTO group = await _cardapioUnitOfWork.GrupoRepo.GetGrupoByIDAsync(groupID);

            return group;
        }

        private async Task<UsuarioGetDTO> GetEmpresaIDAsync(int usuarioID)
        {
            UsuarioGetDTO user = await _cardapioUnitOfWork.UsuarioRepo.GetUsuarioByIDAsync(usuarioID);

            return user;
        }

        private async Task<int> GetGroupIDAsync(int usuarioID)
        {
            IEnumerable<GrupoGetDTO> group = await _cardapioUnitOfWork.GrupoRepo.GetGrupoByUsuarioIDAsync(usuarioID);

            return group.First().ID;
        }

        private async Task<UserGroup> GetGroupUserIDAsync(int usuarioID)
        {
            UserGroup groupUser = await _cardapioUnitOfWork.GrupoUsuarioRepo.GetGroupByUserID(usuarioID);

            return groupUser;
        }


        public async Task<ActionResult> TrocarSenha(string email, string novaSenha, string codigo)
        {
            var usuario = await _cardapioUnitOfWork.UsuarioRepo.GetByEmailAsync(email);
            if (usuario == null)
                throw new ErrorResponse("E-mail não encontrado.", 404);

            // Verifica o código antes de trocar a senha
            var verificacao = await _codigoVerificacaoService.VerifyCode(new CodigoVerificacaoValueAddDTO
            {
                Email = email,
                Code = codigo
            });

            if (verificacao is not OkObjectResult)
                throw new ErrorResponse("Código inválido ou expirado.", 400);

            usuario.Password = novaSenha;
            await _cardapioUnitOfWork.UsuarioRepo.UpdateAsync(usuario);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

      public async Task<ActionResult> EnviarCodigoRecuperacao(string email)
        {
            var usuario = await _cardapioUnitOfWork.UsuarioRepo.GetByEmailAsync(email);
            if (usuario == null)
                throw new ErrorResponse("E-mail não encontrado.", 404);

            return await _codigoVerificacaoService.SendCode(new CodigoVerificacaoToAddDTO { Email = email });
        }
    }
}
