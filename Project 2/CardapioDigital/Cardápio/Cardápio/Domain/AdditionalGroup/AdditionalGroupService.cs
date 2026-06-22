using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Cardápio.Infra.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cardápio.Domain.AdditionalGroup;



namespace Cardápio.Domain.AdditionalGroup
{

    
    public class AdditionalGroupService
    {
        private Dictionary<string, string> validationErrors = new();
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;
        private readonly AppPathsService _appPathsService;

        public AdditionalGroupService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext, Validator validator, AppPathsService appPathsService)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
            _appPathsService = appPathsService;
        }

        public async Task<ActionResult> GetDataAdditionalGroup(ClaimsPrincipal User, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            IEnumerable<GrupoAdicionalGetDTO> grupos = await _cardapioUnitOfWork.GrupoAdicionalItemRepo.GetAdditionalGroupByEmpresaID(empresaID);

            return new OkObjectResult(grupos);
        }

        public async Task<ActionResult> GetDataAdditionalGroupByProductID(ClaimsPrincipal User, int productID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            IEnumerable<GrupoAdicionalGetDTO> grupos = await _cardapioUnitOfWork.GrupoAdicionalItemRepo.GetAdditionalGroupByProdutoID(productID);

            return new OkObjectResult(grupos);
        }

        public async Task<ActionResult> GetDataAdditionalGroupWithProduct(ClaimsPrincipal User, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            IEnumerable<GrupoAdicionalGetDTO> grupos = await _cardapioUnitOfWork.GrupoAdicionalItemRepo.GetAdditionalGroupWithProductByEmpresaID(empresaID);

            return new OkObjectResult(grupos);
        }

        public async Task<ActionResult> VerifyProducts(ClaimsPrincipal User, int groupID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            IEnumerable<ProdutoGetDTO> products = await _cardapioUnitOfWork.ProdutoRepo.GetProdutoByGrupoAdicionalID(groupID);

            return new OkObjectResult(products);
        }

        public async Task<ActionResult> SaveNewGroupAdditional(ClaimsPrincipal User, GrupoAdicionalAddDTO grupoAdicionalAdd)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            string baseName = grupoAdicionalAdd.Nome;
            string finalName = await _cardapioUnitOfWork.GrupoAdicionalRepo.GetUniqueGroupName(baseName, grupoAdicionalAdd.EmpresaID ?? 0);

            GrupoAdicional additionalGroup = CreateAdditionalGroup(grupoAdicionalAdd, UserID);
            additionalGroup.Nome = finalName;

            await _cardapioUnitOfWork.GrupoAdicionalRepo.AddAsync(additionalGroup);
            await _cardapioUnitOfWork.Commit();

            foreach (var item in grupoAdicionalAdd.ColGrupoAdicionalItem)
            {
                GrupoAdicionalItem additionalGroupItem = CreateAdditionalGroupItem(item, grupoAdicionalAdd.EmpresaID, additionalGroup.ID, UserID);
                await _cardapioUnitOfWork.GrupoAdicionalItemRepo.AddAsync(additionalGroupItem);
                await _cardapioUnitOfWork.Commit();

                GrupoAdicionalItemImagem additionalGroupItemImage = await CreateOrUpdateAdditionalGroupItemImage(item, additionalGroupItem.ID, UserID);
                await _cardapioUnitOfWork.GrupoAdicionalItemImagemRepo.AddAsync(additionalGroupItemImage);
                await _cardapioUnitOfWork.Commit();
            }

            return new OkObjectResult(additionalGroup.ID);
        }

        public async Task<ActionResult> EditGroupAdditional(ClaimsPrincipal User, GrupoAdicionalUpdateDTO grupoAdicionalUpdate)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            UsuarioGetDTO user = await GetEmpresaIDAsync(UserID ?? 0);
            GrupoAdicional grupoAdicionalDbo = await _cardapioUnitOfWork.GrupoAdicionalRepo.GetGrupoAdicionalAsyncByIdAndEmpresaID(grupoAdicionalUpdate.ID, user.EmpresaID ?? 0);

            string baseName = grupoAdicionalUpdate.Nome;
            string finalName = await _cardapioUnitOfWork.GrupoAdicionalRepo.GetUniqueGroupName(baseName, grupoAdicionalUpdate.EmpresaID ?? 0, grupoAdicionalUpdate.ID);
            grupoAdicionalDbo.Nome = finalName;

            UpdateAdditionalGroup(grupoAdicionalDbo, grupoAdicionalUpdate);
            await _cardapioUnitOfWork.GrupoAdicionalRepo.UpdateAsync(grupoAdicionalDbo);

            var existingItemIds = grupoAdicionalDbo.Produtos.Select(p => p.ID).ToList();

            var updatedItemIds = grupoAdicionalUpdate.ColGrupoAdicionalItem.Select(i => i.ID).ToList();

            var itemsToDeactivate = existingItemIds.Except(updatedItemIds).ToList();
            foreach (var itemId in itemsToDeactivate)
            {
                var itemToDeactivate = grupoAdicionalDbo.Produtos.FirstOrDefault(p => p.ID == itemId);
                if (itemToDeactivate != null)
                {
                    itemToDeactivate.Ativo = false;
                    itemToDeactivate.DataEdicao = DateTime.Now;
                }
            }

            foreach (GrupoAdicionalItemAddDTO additional in grupoAdicionalUpdate.ColGrupoAdicionalItem)
            {
                if (additional.IsNovo)
                {
                    GrupoAdicionalItem additionalGroupItem = CreateAdditionalGroupItem(additional, grupoAdicionalUpdate.EmpresaID, grupoAdicionalUpdate.ID, UserID);
                    await _cardapioUnitOfWork.GrupoAdicionalItemRepo.AddAsync(additionalGroupItem);
                    await _cardapioUnitOfWork.Commit();

                    GrupoAdicionalItemImagem additionalGroupItemImage = await CreateOrUpdateAdditionalGroupItemImage(additional, additionalGroupItem.ID, UserID);
                    await _cardapioUnitOfWork.GrupoAdicionalItemImagemRepo.AddAsync(additionalGroupItemImage);
                }
                else
                {
                    UpdateExistingAdditionalGroupItem(grupoAdicionalDbo, additional, UserID);
                    await _cardapioUnitOfWork.GrupoAdicionalRepo.UpdateAsync(grupoAdicionalDbo);
                }
            }

            await _cardapioUnitOfWork.Commit();
            return new OkResult();
        }

        public async Task<ActionResult> UnlinkGroupFromProduct(ClaimsPrincipal User, int grupoID, int produtoID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            ProdutoGrupoAdicional produtoGrupoAdicional = await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo.GetByIDAndProductID(grupoID, produtoID);

            if (produtoGrupoAdicional != null)
            {
                produtoGrupoAdicional.Ativo = false;

                await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo.UpdateAsync(produtoGrupoAdicional);
                await _cardapioUnitOfWork.Commit();
            }

            return new OkResult();
        }

        public async Task<ActionResult> DeleteGroupAdditional(ClaimsPrincipal User, int grupoID, int empresaID) {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            GrupoAdicional grupoAdicionalDbo = await _cardapioUnitOfWork.GrupoAdicionalRepo.GetGrupoAdicionalAsyncByIdAndEmpresaID(grupoID, empresaID);

            grupoAdicionalDbo.Ativo = false;
            await _cardapioUnitOfWork.GrupoAdicionalRepo.UpdateAsync(grupoAdicionalDbo);

            foreach (var item in grupoAdicionalDbo.Produtos)
            {
                item.Ativo = false;
                await _cardapioUnitOfWork.GrupoAdicionalItemRepo.UpdateAsync(item);

                foreach(var imagem in item.ColImagemAdicional)
                {
                    if (imagem.Arquivo != "Default.png" && imagem.Nome != "Default.png")
                    {
                        DeleteImageFile(imagem.Arquivo);
                    }

                    imagem.Ativo = false;
                    await _cardapioUnitOfWork.GrupoAdicionalItemImagemRepo.UpdateAsync(imagem);
                }
            }

            foreach(var relation in grupoAdicionalDbo.ProdutoGruposAdicional)
            {
                relation.Ativo = false;
                await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo.UpdateAsync(relation);
            }

            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        private GrupoAdicional CreateAdditionalGroup(GrupoAdicionalAddDTO grupoAdicionalAdd, int? UserID)
        {
            return new Infra.Model.GrupoAdicional
            {
                Ativo = true,
                EmpresaID = grupoAdicionalAdd.EmpresaID,
                Maximo = grupoAdicionalAdd.Maximo ?? 0,
                Minimo = grupoAdicionalAdd.Minimo ?? 0,
                Nome = grupoAdicionalAdd.Nome,
                TipoID = grupoAdicionalAdd.TipoId ?? 0,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
            };
        }

        private GrupoAdicionalItem CreateAdditionalGroupItem(GrupoAdicionalItemAddDTO item, int? empresaID, int grupoAdicionalID, int? UserID)
        {
            return new Infra.Model.GrupoAdicionalItem
            {
                Nome = item.Nome,
                Preco = item.Preco,
                EmpresaID = empresaID,
                Ativo = true,
                GrupoAdicionalID = grupoAdicionalID,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
            };
        }

        private async Task<GrupoAdicionalItemImagem> CreateOrUpdateAdditionalGroupItemImage(GrupoAdicionalItemAddDTO item, int grupoAdicionalItemID, int? UserID)
        {
            string arquivoNome = !string.IsNullOrEmpty(item.ImageID) ? MoveImageFile(item.ImageID) : "Default.png";
            return new Infra.Model.GrupoAdicionalItemImagem
            {
                Altura = 60,
                Largura = 60,
                Arquivo = arquivoNome,
                Nome = arquivoNome,
                Ativo = true,
                GrupoAdicionalItemID = grupoAdicionalItemID,
                Tamanho = 0,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
            };
        }

        private string MoveImageFile(string imageID)
        {
            
            
            // Extrai guid e extensão corretamente
            string fileName = Path.GetFileName(imageID);
            var guidMatch = System.Text.RegularExpressions.Regex.Match(fileName, @"([a-fA-F0-9\-]{36})");
            var extMatch = System.Text.RegularExpressions.Regex.Match(fileName, @"\.([a-zA-Z0-9]+)$");
            string guid = guidMatch.Success ? guidMatch.Value : string.Empty;
            string ext = extMatch.Success ? extMatch.Value : string.Empty;

           

            // Se não encontrar extensão, tenta buscar arquivo físico para descobrir
            if (string.IsNullOrEmpty(ext) && !string.IsNullOrEmpty(guid))
            {
                // Diretório de imagens adicionais
                var dir = _appPathsService.GetAdditionalImagesDirectory();
                var files = Directory.GetFiles(dir, guid + ".*");
                if (files.Length > 0)
                {
                    ext = Path.GetExtension(files[0]);
                    
                }
            }

            // Monta nome final guid+extensão
            if (!string.IsNullOrEmpty(guid) && !string.IsNullOrEmpty(ext))
            {
                fileName = guid + ext;
            }

            string oldFilePath = Path.Combine(_appPathsService.GetAdditionalImagesDirectory(), imageID);
            string newFilePath = Path.Combine(_appPathsService.GetAdditionalImagesDirectory(), fileName);

           

            if (File.Exists(oldFilePath))
            {
              
                File.Move(oldFilePath, newFilePath);
                RedimensionarImagem.Redimensionar(newFilePath, newFilePath, 60, 60);
            }
            else
            {
                Console.WriteLine($"[AdditionalGroup] ❌ Arquivo não encontrado: {oldFilePath}");
            }

            return fileName;
        }

        private void UpdateAdditionalGroup(GrupoAdicional grupoAdicionalDbo, GrupoAdicionalUpdateDTO grupoAdicionalUpdate)
        {
            grupoAdicionalDbo.DataEdicao = DateTime.Now;
            grupoAdicionalDbo.Maximo = grupoAdicionalUpdate.Maximo ?? 0;
            grupoAdicionalDbo.Minimo = grupoAdicionalUpdate.Minimo ?? 0;
            grupoAdicionalDbo.TipoID = grupoAdicionalUpdate.TipoId ?? 0;
        }

        private void UpdateExistingAdditionalGroupItem(GrupoAdicional grupoAdicionalDbo, GrupoAdicionalItemAddDTO additional, int? UserID)
        {
            var produtoExistente = grupoAdicionalDbo.Produtos.FirstOrDefault(p => p.ID == additional.ID);
            if (produtoExistente == null) return;

            produtoExistente.Preco = additional.Preco;
            produtoExistente.Nome = additional.Nome;

            var imagemAtual = produtoExistente.ColImagemAdicional.FirstOrDefault();

            // 🔧 CORREÇÃO: Permitir atualização quando ImageID for removido (vazio/null) ou alterado
            if (imagemAtual != null)
            {
                string novoImageID = string.IsNullOrEmpty(additional.ImageID) ? "Default.png" : additional.ImageID;
                
                // Só processa se a imagem mudou
                if (imagemAtual.Nome != novoImageID)
                {
                    // Remove imagem antiga se não for Default.png
                    if (!string.IsNullOrEmpty(imagemAtual.Nome) && imagemAtual.Nome != "Default.png" && 
                        !string.IsNullOrEmpty(imagemAtual.Arquivo) && imagemAtual.Arquivo != "Default.png")
                    {
                        DeleteImageFile(imagemAtual.Nome);
                    }

                    // Define nova imagem
                    string arquivoNome = !string.IsNullOrEmpty(additional.ImageID) ? MoveImageFile(additional.ImageID) : "Default.png";
                    
                    imagemAtual.Nome = arquivoNome;
                    imagemAtual.Arquivo = arquivoNome;
                    imagemAtual.Altura = 60;
                    imagemAtual.Largura = 60;
                    imagemAtual.DataEdicao = DateTime.Now;
                }
            }

            produtoExistente.DataEdicao = DateTime.Now;
        }

        private void DeleteImageFile(string imageName)
        {
            string filePath = Path.Combine(_appPathsService.GetAdditionalImagesDirectory(), imageName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private async Task<UsuarioGetDTO> GetEmpresaIDAsync(int usuarioID)
        {
            UsuarioGetDTO user = await _cardapioUnitOfWork.UsuarioRepo.GetUsuarioByIDAsync(usuarioID);

            return user;
        }
    }
}
