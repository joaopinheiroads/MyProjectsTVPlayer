using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Cardápio.Dto;
using Cardápio.Client.Infra.Validation;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Cardápio.Infra.Data;

namespace Cardápio.Domain.Category
{
    public class CategoryService
    {
        private Dictionary<string, string> validationErrors = new();
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;

        public CategoryService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext, Validator validator)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task<ActionResult> GetDataByCompanyID(ClaimsPrincipal User, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            _validator.VerifyCategoryIdIsPresent(empresaID);

            IEnumerable<CategoriaGetDTO> categorias = await _cardapioUnitOfWork.CategoriaRepo.GetCategoriaByEmpresaIDAsyncNotAuthenticated(empresaID);

            return new OkObjectResult(categorias);
        }

        public async Task<ActionResult> GetDataByCompanyName(string companyName)
        {
            IEnumerable<CategoriaGetDTO> categorias = await _cardapioUnitOfWork.CategoriaRepo.GetCategoriaByEmpresaIDAsyncNotAuthenticated(companyName);

            return new OkObjectResult(categorias);
        }

        public async Task<ActionResult> UpdateCategoriaOrdemAsync(ClaimsPrincipal User, ICollection<CategoriaUpdateDTO> categoriasAtualizadas)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            foreach (CategoriaUpdateDTO categoriaAtualizada in categoriasAtualizadas)
            {
                Infra.Model.Category categoria = await _appDbContext.Categoria.FindAsync(categoriaAtualizada.ID);

                _validator.VerifyCategoryIdIsPresent(categoria.ID);

                categoria.Ordem = categoriaAtualizada.Ordem;
                await _cardapioUnitOfWork.CategoriaRepo.UpdateAsync(categoria);
            }

            await _appDbContext.SaveChangesAsync();

            return new OkObjectResult("Ordens das categorias atualizadas com sucesso.");
        }

        public async Task<ActionResult> AddCategoriaAsync(ClaimsPrincipal User, CategoriaAddDTO categoriaAddDTO, int empresaID)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            _validator.VerifyCategoryIdIsPresent(empresaID);

            Client.Dto.VerifyCategoryDTO verifyCategoryDTO = new Client.Dto.VerifyCategoryDTO()
            {
                ID = null,
                Nome = categoriaAddDTO.Nome,
            };

            await VerifyCategoryAlreadyExists(UserID, verifyCategoryDTO, empresaID);

            Infra.Model.Category categoria = new Infra.Model.Category
            {
                Nome = categoriaAddDTO.Nome,
                BackgroundCor = ConvertRgbToHex(categoriaAddDTO.BackgroundColor),
                Ordem = await _cardapioUnitOfWork.CategoriaRepo.GetNextOrdemByEmpresaIDAsync(empresaID),
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                EmpresaID = empresaID,
                DataEdicao = DateTime.UtcNow,
                Ativo = true
            };

            await _cardapioUnitOfWork.CategoriaRepo.AddAsync(categoria);
            await _cardapioUnitOfWork.Commit();

            return new CreatedResult();
        }

        public async Task<ActionResult> UpdateCategoriaAsync(ClaimsPrincipal User, int categoriaID, CategoriaUpdateDTO categoriaUpdateDTO)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCompanyIdIsPresent(usuario.EmpresaID);

            Client.Dto.VerifyCategoryDTO verifyCategoryDTO = new Client.Dto.VerifyCategoryDTO()
            {
                ID = categoriaID,
                Nome = categoriaUpdateDTO.Nome,
            };

            await VerifyCategoryAlreadyExists(UserID, verifyCategoryDTO, usuario.EmpresaID ?? 0);

            Infra.Model.Category categoria = await _cardapioUnitOfWork.CategoriaRepo.GetByIDAsync(categoriaID, usuario.EmpresaID ?? 0);
            _validator.VerifyCategoryIdIsPresent(categoria.ID);

            categoria.ID = categoriaUpdateDTO.ID;
            categoria.Nome = categoriaUpdateDTO.Nome;
            categoria.Ordem = categoria.Ordem;
            categoria.EmpresaID = categoria.EmpresaID;
            categoria.DataEdicao = new DateTime();
            categoria.DataCadastro = categoria.DataCadastro;
            categoria.UsuarioIDEdicao = UserID;
            categoria.UsuarioIDCadastro = categoria.UsuarioIDCadastro;
            categoria.BackgroundCor = ConvertRgbToHex(categoriaUpdateDTO.BackgroundColor);
            categoria.Ativo = true;

            await _cardapioUnitOfWork.CategoriaRepo.UpdateAsync(categoria);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> VerifyCategoryAlreadyExists(int userID, Client.Dto.VerifyCategoryDTO verifyCategoryDTO, int empresaID)
        {
            _validator.VerifyCategoryIdIsPresent(userID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(userID);

            IEnumerable<Infra.Model.Category> queryCategorias = _appDbContext.Categoria.Where(c => c.Nome == verifyCategoryDTO.Nome && c.Ativo && c.EmpresaID == empresaID).ToList();

            if (verifyCategoryDTO.ID != null)
            {
                queryCategorias = queryCategorias.Where(c => c.ID != verifyCategoryDTO.ID);
            }

            foreach (Infra.Model.Category categoria in queryCategorias)
            {
                _validator.VerifyCategoriaAlreadyExists(categoria);
            }

            return new OkResult();
        }

        public async Task<ActionResult> DeleteCategoriaAsync(ClaimsPrincipal User, int categoriaID, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            _validator.VerifyCategoryIdIsPresent(empresaID);

            Infra.Model.Category categoria = await _cardapioUnitOfWork.CategoriaRepo.GetByIDAsync(categoriaID, empresaID);
            _validator.VerifyCategoryIdIsPresent(categoria.ID);

            categoria.UsuarioIDEdicao = UserID;
            categoria.Ativo = false;

            ICollection<Infra.Model.Category> colCategoria = await _cardapioUnitOfWork.CategoriaRepo.GetByBiggerThenOrdemAsync(categoria.Ordem.GetValueOrDefault(), empresaID);
            foreach (Infra.Model.Category categoriaOrdemSuperior in colCategoria)
            {
                categoriaOrdemSuperior.Ordem -= 1;
                categoriaOrdemSuperior.UsuarioIDEdicao = UserID;
            }

            await _cardapioUnitOfWork.CategoriaRepo.UpdateAsync(categoria);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
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

        private string ConvertRgbToHex(string rgb)
        {
            if (rgb.StartsWith("#"))
            {
                return rgb;
            }

            var rgbValues = rgb.Replace("rgb(", "").Replace(")", "").Split(',');
            int rValue = int.Parse(rgbValues[0].Trim());
            int gValue = int.Parse(rgbValues[1].Trim());
            int bValue = int.Parse(rgbValues[2].Trim());
            return $"#{rValue:X2}{gValue:X2}{bValue:X2}";
        }

        private async Task<bool> ValidateEntityAsync<GenericValidate>(GenericValidate entity, params string[] requiredFields)
        {
            var validator = new ValidationService<GenericValidate>();
            var validationResult = validator.Validate(entity, requiredFields);

            if (!validationResult.IsValid)
            {
                validationErrors = validationResult.Errors;
                return false;
            }

            validationErrors.Clear();
            return true;
        }
    }
}
