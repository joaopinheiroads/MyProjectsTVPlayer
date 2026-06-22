using System.Diagnostics;
using System.Security.Claims;
using Cardápio.Domain.ProdutoHorario;
using Cardápio.Domain.ProdutoPromocaoHorario;
using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Cardápio.Infra.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using ImageSharp = SixLabors.ImageSharp.Image;
using SizeSharp = SixLabors.ImageSharp.Size;

namespace Cardápio.Domain.Product
{
    public class ProductService
    {
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;
        private readonly AppPathsService _appPathsService;
        private readonly ProdutoHorarioService _produtoHorarioService;
        private readonly IServiceProvider _serviceProvider;

        public ProductService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext, Validator validator, AppPathsService appPathsService, ProdutoHorarioService produtoHorarioService, IServiceProvider serviceProvider)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
            _appPathsService = appPathsService;
            _produtoHorarioService = produtoHorarioService;
            _serviceProvider = serviceProvider;
        }

        public async Task<ActionResult> GetProdutoByEmpresaIDAsync(ClaimsPrincipal User, int empresaNome)
        {

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return new BadRequestObjectResult("Usuário não autenticado.");
            int UserID = int.Parse(userIdString);
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);

            int grupoID = await GetGroupIDAsync(usuario.ID);
            UserGroup groupUser = await GetGroupUserIDAsync(UserID);

            IEnumerable<ProdutoGetDTO> produtos = await _cardapioUnitOfWork.ProdutoRepo.GetProdutoByEmpresaIdAsync(empresaNome, UserID, grupoID, groupUser.IsAdmin);

            return new OkObjectResult(produtos);
        }

        public async Task<ActionResult> GetProdutoByEmpresaNomeAsync(string empresaNome)
        {
            IEnumerable<ProdutoGetDTO> produtos = await _cardapioUnitOfWork.ProdutoRepo.GetProdutoByEmpresaNomeAsync(empresaNome);
            
            // Filtrar produtos que estão disponíveis no horário atual E aplicar promoções
            var produtosDisponiveis = new List<ProdutoGetDTO>();
            
            foreach (var produto in produtos)
            {
                var disponivel = await _produtoHorarioService.IsProdutoDisponivelNoHorario(produto.ID);
                
                if (disponivel)
                {
                    // 🎯 APLICAR PROMOÇÕES POR HORÁRIO
                    try 
                    {
                        var produtoPromocaoHorarioService = _serviceProvider.GetService<ProdutoPromocaoHorarioService>();
                        if (produtoPromocaoHorarioService != null)
                        {
                            var precoPromocional = await produtoPromocaoHorarioService.GetPrecoPromocionalAtual(produto.ID);
                            
                            if (precoPromocional.HasValue)
                            {
                                // Logs removidos para limpeza
                                produto.Promocao = true;
                                produto.PrecoPromocional = precoPromocional.Value;
                                // 🎯 MANTER preço original em produto.Preco para aparecer riscado
                                // 🎯 PrecoPromocional fica com valor da promoção
                            }
                            else
                            {
                                // Log removido para limpeza
                                produto.Promocao = false;
                                produto.PrecoPromocional = null;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Em caso de erro, não aplicar promoção
                        produto.Promocao = false;
                        produto.PrecoPromocional = null;
                    }
                    
                    produtosDisponiveis.Add(produto);
                }
            }

            return new OkObjectResult(produtosDisponiveis);
        }

        public async Task<ActionResult> GetProdutoByGroupID(ClaimsPrincipal User, int grupoID)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            IEnumerable<ProdutoGetDTO> produtos = await _cardapioUnitOfWork.ProdutoRepo.GetProdutoByGrupoID(grupoID);

            return new OkObjectResult(produtos);
        }

        public async Task<ActionResult> GetProdutoByID(ClaimsPrincipal User, int productID)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);

            if (usuario == null)
            {
                throw new ErrorResponse("Usuário não encontrado.", 404);
            }

            ProdutoGetDTO? produto = await _cardapioUnitOfWork.ProdutoRepo.GetProdutoByIDAsync(productID, usuario.EmpresaID ?? 0);

            if (produto == null)
            {
                throw new ErrorResponse("Produto não encontrado.", 404);
            }

            // 🎯 APLICAR PROMOÇÕES POR HORÁRIO TAMBÉM AQUI!
            try 
            {
                var produtoPromocaoHorarioService = _serviceProvider.GetService<ProdutoPromocaoHorarioService>();
                if (produtoPromocaoHorarioService != null)
                {
                    var precoPromocional = await produtoPromocaoHorarioService.GetPrecoPromocionalAtual(produto.ID);
                    
                    if (precoPromocional.HasValue)
                    {
                        produto.Promocao = true;
                        produto.PrecoPromocional = precoPromocional.Value;
                        // 🎯 MANTER preço original em produto.Preco para aparecer riscado no frontend
                        // 🎯 PrecoPromocional fica com valor da promoção
                    }
                    else
                    {
                        produto.Promocao = false;
                        produto.PrecoPromocional = null;
                    }
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, não aplicar promoção
                produto.Promocao = false;
                produto.PrecoPromocional = null;
            }

            return new OkObjectResult(produto);
        }

        // 🔓 MÉTODO PÚBLICO para cardápio - SEM autenticação necessária
        public async Task<ActionResult> GetProdutoByIDPublic(int productID, string empresaNome)
        {
            // Buscar empresa pelo nome
            var empresa = await _cardapioUnitOfWork.EmpresaRepo.GetEmpresaByNameAsync(empresaNome);
            if (empresa == null)
            {
                throw new ErrorResponse("Empresa não encontrada.", 404);
            }

            ProdutoGetDTO? produto = await _cardapioUnitOfWork.ProdutoRepo.GetProdutoByIDAsync(productID, empresa.ID);

            if (produto == null)
            {
                throw new ErrorResponse("Produto não encontrado.", 404);
            }

            // 🎯 APLICAR PROMOÇÕES POR HORÁRIO
            try 
            {
                var produtoPromocaoHorarioService = _serviceProvider.GetService<ProdutoPromocaoHorarioService>();
                if (produtoPromocaoHorarioService != null)
                {
                    var precoPromocional = await produtoPromocaoHorarioService.GetPrecoPromocionalAtual(produto.ID);
                    
                    if (precoPromocional.HasValue)
                    {
                        produto.Promocao = true;
                        produto.PrecoPromocional = precoPromocional.Value;
                        // 🎯 MANTER preço original em produto.Preco para aparecer riscado no frontend
                        // 🎯 PrecoPromocional fica com valor da promoção  
                    }
                    else
                    {
                        produto.Promocao = false;
                        produto.PrecoPromocional = null;
                    }
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, não aplicar promoção
                produto.Promocao = false;
                produto.PrecoPromocional = null;
            }

            return new OkObjectResult(produto);
        }

        public async Task<ActionResult> SaveProductThumb(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("Nenhuma imagem selecionada");
            }

            string uniqueFileName = $"temp_{Guid.NewGuid()}.png";

            string thumbnailPath = Path.Combine(_appPathsService.GetThumbnailsDirectory(), uniqueFileName);
            string fullImagePath = Path.Combine(_appPathsService.GetProductImagesDirectory(), uniqueFileName);


            try
            {
                await SaveResizedImage(file, fullImagePath, new SizeSharp(500, 0));
                await SaveResizedImage(file, thumbnailPath, new SizeSharp(300, 0));

                return new OkObjectResult(new { FileName = uniqueFileName });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult($"Erro ao salvar imagem: {ex.Message}");
            }
        }

        private async Task SaveResizedImage(IFormFile file, string path, SizeSharp size)
        {
            var a = file.OpenReadStream();
            a.Position = 0;

            using (var image = await ImageSharp.LoadAsync(a))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = size
                }));

                await image.SaveAsync(path);
            }
        }

        public async Task<ActionResult> AddProducts(ClaimsPrincipal User, ProdutoAddDTO product, int selectedEmpresaID)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCompanyIdIsPresent(usuario.EmpresaID);
            _validator.VerifyCompanyIdIsPresent(selectedEmpresaID);

            Client.Dto.VerifyProductDTO verifyProductDTO = new Client.Dto.VerifyProductDTO()
            {
                ID = null,
                CategoriaID = product.CategoriaID,
                ProdutoNome = product.Nome,
            };

            await VerifyProductAlreadyExists(UserID, verifyProductDTO, usuario.EmpresaID ?? 0);

            Infra.Model.Category category = GetCategoryAsync(product.CategoriaID);
            _validator.VerifyCategoryIdIsPresent(category.ID);

            Infra.Model.Product productModel = CreateProductModel(product, UserID, selectedEmpresaID, category);
            productModel = await AddProductAsync(productModel);

            ImageProduct productImage = await CreateOrUpdateProductItemImage(product, UserID, productModel.ID);
            ThumbnailProduct productThumb = CreateThumbFile(productImage.Nome.Replace("temp_", ""), UserID);

            await _cardapioUnitOfWork.ProdutoImagemRepo.AddAsync(productImage);
            await _cardapioUnitOfWork.Commit();

            await _cardapioUnitOfWork.ProdutoThumbnailRepo.AddAsync(productThumb);
            await _cardapioUnitOfWork.Commit();

            productModel.ProdutoThumbnailID = productThumb.ID;
            await _cardapioUnitOfWork.ProdutoRepo.UpdateAsync(productModel);
            await _cardapioUnitOfWork.Commit();

            foreach (var grupo in product.GruposOrdenados.OrderBy(g => g.Posicao))
            {
                ProdutoGrupoAdicional produtoGrupoAdicional = new ProdutoGrupoAdicional
                {
                    GrupoAdicionalID = grupo.GrupoAdicionalID,
                    ProdutoID = productModel.ID,
                    Ativo = true,
                    Posicao = grupo.Posicao
                };

                await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo.AddAsync(produtoGrupoAdicional);
            }

            // Adicionar horários do produto se especificados
            if (product.Horarios != null && product.Horarios.Any())
            {
                foreach (var horario in product.Horarios)
                {
                    var result = await _produtoHorarioService.Add(new ProdutoHorarioAddDTO
                    {
                        ProdutoID = productModel.ID,
                        DiaSemana = horario.DiaSemana,
                        HoraInicio = horario.HoraInicio,
                        HoraFim = horario.HoraFim,
                        Ativo = horario.Ativo
                    }, UserID);
                }
            }

            // Adicionar promoções por horário do produto se especificadas
            if (product.PromocoesPorHorario != null && product.PromocoesPorHorario.Any())
            {
                foreach (var promocao in product.PromocoesPorHorario)
                {
                    
                    var promocaoModel = new Infra.Model.ProdutoPromocaoHorario
                    {
                        ProdutoID = productModel.ID,
                        DiaSemana = promocao.DiaSemana,
                        HoraInicio = TimeSpan.Parse(promocao.HoraInicio),
                        HoraFim = TimeSpan.Parse(promocao.HoraFim),
                        PrecoPromocional = promocao.PrecoPromocional,
                        Ativo = true,
                        DataCadastro = DateTime.UtcNow,
                        DataEdicao = DateTime.UtcNow,
                        UsuarioIDCadastro = UserID,
                        UsuarioIDEdicao = UserID
                    };
                    
                    await _cardapioUnitOfWork.ProdutoPromocaoHorarioRepo.AddAsync(promocaoModel);
                }
            }

            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        private async Task<ImageProduct> CreateOrUpdateProductItemImage(ProdutoAddDTO item, int? UserID, int produtoID)
        {
            string arquivoNome = !string.IsNullOrEmpty(item.ImagesID.FirstOrDefault()) ? MoveImageFile(item.ImagesID.FirstOrDefault()) : "Default.png";

            return new Infra.Model.ImageProduct
            {
                Altura = 120,
                Largura = 120,
                Arquivo = arquivoNome,
                Nome = arquivoNome,
                Ativo = true,
                ProdutoID = produtoID,
                Tamanho = 0,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
            };
        }

        private string MoveImageFile(string imageID)
        {
            
            try
            {
                // Verificar se é um imageID válido
                if (string.IsNullOrEmpty(imageID))
                {
                    return "Default.png";
                }
                
                // Procurar arquivo temporário na pasta temp
                var tempDir = Path.Combine(_appPathsService.GetImagesDirectory(), "temp");
                
                // Garantir que o diretório temp existe
                if (!Directory.Exists(tempDir))
                {
                    return "Default.png";
                }
                
                var tempFiles = Directory.GetFiles(tempDir, $"{imageID}_*");
                
                
                if (tempFiles.Length == 0)
                {
                    // Tentar procurar com o nome exato do arquivo
                    var allTempFiles = Directory.GetFiles(tempDir);
                    foreach (var file in allTempFiles)
                    {
                        Console.WriteLine($"[MoveImageFile] - {Path.GetFileName(file)}");
                    }
                    
                    return "Default.png";
                }
                
                var tempFilePath = tempFiles[0];
                var tempFileName = Path.GetFileName(tempFilePath);
                var extension = Path.GetExtension(tempFilePath);
                
                // Gerar nome mais curto para não ultrapassar limite de 50 caracteres do banco
                // Formato: p_HHMMSS_primeiros8digitos.ext (max ~20 chars)
                var timestamp = DateTime.Now.ToString("HHmmss");
                var shortId = imageID.Replace("-", "").Substring(0, Math.Min(8, imageID.Replace("-", "").Length));
                var newFileName = $"p_{timestamp}_{shortId}{extension}";
                
                // Verificar se o nome não ultrapassa 50 caracteres
                if (newFileName.Length > 50)
                {
                    // Se ainda for muito longo, usar apenas timestamp + extensão
                    newFileName = $"p_{timestamp}{extension}";
                }
                
                var newFilePath = Path.Combine(_appPathsService.GetProductImagesDirectory(), newFileName);
                

                if (File.Exists(tempFilePath))
                {
                    // Garantir que o diretório de destino existe
                    Directory.CreateDirectory(_appPathsService.GetProductImagesDirectory());
                    
                    // Se o arquivo de destino já existe, removê-lo
                    if (File.Exists(newFilePath))
                    {
                        File.Delete(newFilePath);
                    }
                    
                    // Mover arquivo da pasta temp para ImageProducts
                    File.Move(tempFilePath, newFilePath);
                    
                    return newFileName;
                }
                else
                {
                    return "Default.png";
                }
            }
            catch (Exception ex)
            {
                return "Default.png";
            }
        }

        private ThumbnailProduct CreateThumbFile(string imageID, int? UserID)
        {
            string arquivoNome = !string.IsNullOrEmpty(imageID) ? imageID.Replace("temp_", "") : "Default.png";

            if (arquivoNome != "Default.png")
            {
                string imageDirectory = _appPathsService.GetProductImagesDirectory();
                string thumbnailDirectory = _appPathsService.GetThumbnailsDirectory();

                string imageName = imageID.Replace("temp_", "");
                string imagePath = Path.Combine(imageDirectory, imageName);
                string thumbnailPath = Path.Combine(thumbnailDirectory, imageName);


                using (var image = SixLabors.ImageSharp.Image.Load(imagePath))
                {
                    var thumbnail = image.Clone(x => x.Resize(250, 250));

                    thumbnail.Save(thumbnailPath);
                }
            }

            return new Infra.Model.ThumbnailProduct
            {
                Altura = 120,
                Largura = 120,
                Arquivo = arquivoNome,
                Nome = arquivoNome,
                Ativo = true,
                Tamanho = 0,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
            };
        }

        private void DeleteImageFile(string imageName, string thumbName)
        {
            string imagePath = Path.Combine(_appPathsService.GetProductImagesDirectory(), imageName);
            string thumbPath = Path.Combine(_appPathsService.GetThumbnailsDirectory(), thumbName);


            if (File.Exists(imagePath) && imageName != "Default.png")
            {
                File.Delete(imagePath);
            }

            if (File.Exists(thumbPath) && thumbName != "Default.png")
            {
                File.Delete(thumbPath);
            }
        }

        /// <summary>
        /// Verifica se a string é um GUID válido
        /// </summary>
        private bool IsGuid(string value)
        {
            return Guid.TryParse(value, out _);
        }

        public async Task<ActionResult> UpdateProducts(ClaimsPrincipal User, ProdutoAddDTO product, int productID)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            
            if (product.PromocoesPorHorario != null)
            {
                foreach (var promo in product.PromocoesPorHorario)
                {
                    Console.WriteLine($"[UpdateProducts] Promoção: {promo.DiaSemana} {promo.HoraInicio}-{promo.HoraFim} R$ {promo.PrecoPromocional:F2}");
                }
            }
            else
            {
                Console.WriteLine($"[UpdateProducts] ❌ PromocoesPorHorario é NULL");
            }

            if (product.GruposOrdenados != null)
            {
                var gruposFromFrontend = product.GruposOrdenados.ToList();
                for (int i = 0; i < gruposFromFrontend.Count; i++)
                {
                    var grupo = gruposFromFrontend[i];
                }
            }
            else
            {
            }

            _validator.VerifyCategoryIdIsPresent(UserID);

            Client.Dto.VerifyProductDTO verifyProductDTO = new Client.Dto.VerifyProductDTO()
            {
                ID = productID,
                CategoriaID = product.CategoriaID,
                ProdutoNome = product.Nome,
            };

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCompanyIdIsPresent(usuario.EmpresaID);

            await VerifyProductAlreadyExists(UserID, verifyProductDTO, usuario.EmpresaID ?? 0);

            Infra.Model.Category category = GetCategoryAsync(product.CategoriaID);
            _validator.VerifyCategoryIdIsPresent(category.ID);

            Infra.Model.Product productDb = await GetProductByID(productID, usuario.EmpresaID ?? 0);
            _validator.VerifyProductIdIsPresent(usuario.EmpresaID);

            productDb = CreateProductUpdateModel(product, productDb, UserID, category);
            await _cardapioUnitOfWork.ProdutoRepo.UpdateAsync(productDb);

            var allRelations = await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo
                .GetByProductIDAsync(productDb.ID);

            foreach (var rel in allRelations)
            {
                Console.WriteLine($"[UpdateProducts] Relação existente: GrupoID={rel.GrupoAdicionalID}, Posição={rel.Posicao}, Ativo={rel.Ativo}");
            }

            var gruposList = product.GruposOrdenados?.ToList() ?? new List<GrupoOrdenadoDTO>();
            
            for (int i = 0; i < gruposList.Count; i++)
            {
                var grupo = gruposList[i];
                
                var existingRelation = allRelations.FirstOrDefault(pg =>
                    pg.GrupoAdicionalID == grupo.GrupoAdicionalID &&
                    pg.ProdutoID == productDb.ID);

                if (existingRelation != null)
                {
                    existingRelation.Posicao = i + 1;
                    existingRelation.Ativo = true;
                    await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo.UpdateAsync(existingRelation);
                }
                else
                {
                    var newRelation = new ProdutoGrupoAdicional
                    {
                        GrupoAdicionalID = grupo.GrupoAdicionalID,
                        ProdutoID = productDb.ID,
                        Ativo = true,
                        Posicao = i + 1
                    };
                    await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo.AddAsync(newRelation);
                }
            }

            var receivedIds = product.GruposOrdenados?.Select(g => g.GrupoAdicionalID).ToList() ?? new List<int>();
            foreach (var relation in allRelations.Where(r => !receivedIds.Contains(r.GrupoAdicionalID ?? 0)))
            {
                relation.Ativo = false;
                await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo.UpdateAsync(relation);
            }

            await _cardapioUnitOfWork.Commit();
            
            // 🔍 VERIFICAÇÃO: Confirmar que os grupos foram realmente salvos
            var groupsAfterCommit = await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo
                .GetByProductIDAsync(productDb.ID);
            foreach (var group in groupsAfterCommit.Where(g => g.Ativo))
            {
                Console.WriteLine($"[UpdateProducts] ✅ Grupo salvo: ID={group.GrupoAdicionalID}, Posição={group.Posicao}");
            }

            // Gerenciar horários do produto
            // Primeiro, remover todos os horários existentes do produto

            try
            {
                List<Infra.Model.ProdutoHorario> horariosExistentes = await _cardapioUnitOfWork.ProdutoHorarioRepo.GetHorariosByProdutoId(productID);
                foreach (Infra.Model.ProdutoHorario horarioExistente in horariosExistentes)
                {
                    horarioExistente.Ativo = false;
                    horarioExistente.DataEdicao = DateTime.Now;
                    horarioExistente.UsuarioIDEdicao = UserID;

                    await _cardapioUnitOfWork.ProdutoHorarioRepo.Update(horarioExistente);

                }

                // Adicionar os novos horários se especificados
                if (product.Horarios != null && product.Horarios.Any())
                {
                    foreach (var horario in product.Horarios)
                    {
                        var result = await _produtoHorarioService.Add(new ProdutoHorarioAddDTO
                        {
                            ProdutoID = productID,
                            DiaSemana = horario.DiaSemana,
                            HoraInicio = horario.HoraInicio,
                            HoraFim = horario.HoraFim,
                            Ativo = horario.Ativo
                        }, UserID);
                    }
                }
            }
            catch (Exception ex){
                throw;
            }




            try
            {

                List<Infra.Model.ProdutoPromocaoHorario> promocoesExistentes = await _cardapioUnitOfWork.ProdutoPromocaoHorarioRepo.GetPromocoesByProdutoIdAsync(productID);
                
                foreach (var promo in promocoesExistentes.Where(p => p.Ativo))
                {
                    Console.WriteLine($"[UpdateProducts] DB Ativa: {promo.DiaSemana} {promo.HoraInicio}-{promo.HoraFim} R$ {promo.PrecoPromocional:F2}");
                }

                foreach (Infra.Model.ProdutoPromocaoHorario promocaoExistente in promocoesExistentes)
                {
                    if (promocaoExistente.Ativo)
                    {
                        Console.WriteLine($"[UpdateProducts] Desativando: {promocaoExistente.DiaSemana} {promocaoExistente.HoraInicio}-{promocaoExistente.HoraFim}");
                    }
                    promocaoExistente.Ativo = false;
                    promocaoExistente.DataEdicao = DateTime.Now;
                    promocaoExistente.UsuarioIDEdicao = UserID;

                    await _cardapioUnitOfWork.ProdutoPromocaoHorarioRepo.UpdateAsync(promocaoExistente);
                }

                if (product.PromocoesPorHorario != null)
                {
                    foreach (var p in product.PromocoesPorHorario)
                    {
                        Console.WriteLine($"[UpdateProducts] Frontend: {p.DiaSemana} {p.HoraInicio}-{p.HoraFim} R$ {p.PrecoPromocional:F2}");
                    }
                }
                else
                {
                    Console.WriteLine($"[UpdateProducts] ❌ Frontend não enviou promoções (NULL)");
                }

                if (product.PromocoesPorHorario != null && product.PromocoesPorHorario.Any())
                {
                    foreach (ProdutoPromocaoHorarioAddDTO promocaoPorHorario in product.PromocoesPorHorario)
                    {
                        // Verificar se já existe uma promoção com os mesmos dados
                        var promocaoExistenteMatch = promocoesExistentes.FirstOrDefault(p => 
                            p.DiaSemana == promocaoPorHorario.DiaSemana &&
                            p.HoraInicio == TimeSpan.Parse(promocaoPorHorario.HoraInicio) &&
                            p.HoraFim == TimeSpan.Parse(promocaoPorHorario.HoraFim));

                        if (promocaoExistenteMatch != null)
                        {
                            promocaoExistenteMatch.Ativo = true;
                            promocaoExistenteMatch.PrecoPromocional = promocaoPorHorario.PrecoPromocional;
                            promocaoExistenteMatch.DataEdicao = DateTime.Now;
                            promocaoExistenteMatch.UsuarioIDEdicao = UserID;

                            await _cardapioUnitOfWork.ProdutoPromocaoHorarioRepo.UpdateAsync(promocaoExistenteMatch);
                        }
                        else
                        {
                            // Criar nova promoção
                            Infra.Model.ProdutoPromocaoHorario promocaoModel = new Infra.Model.ProdutoPromocaoHorario
                            {
                                ProdutoID = productID,
                                DiaSemana = promocaoPorHorario.DiaSemana,
                                HoraInicio = TimeSpan.Parse(promocaoPorHorario.HoraInicio),
                                HoraFim = TimeSpan.Parse(promocaoPorHorario.HoraFim),
                                PrecoPromocional = promocaoPorHorario.PrecoPromocional,
                                Ativo = true,
                                DataCadastro = DateTime.Now,
                                DataEdicao = null,
                                UsuarioIDCadastro = UserID,
                                UsuarioIDEdicao = null
                            };

                            await _cardapioUnitOfWork.ProdutoPromocaoHorarioRepo.AddAsync(promocaoModel);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[UpdateProducts] ⚠️ Nenhuma promoção para reativar - lista vazia ou null");
                }
                
                Console.WriteLine($"[UpdateProducts] 🎯 === FIM PROCESSAMENTO PROMOÇÕES ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateProducts] ❌ ERRO ao gerenciar promoções: {ex.Message}");
                Console.WriteLine($"[UpdateProducts] ❌ Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[UpdateProducts] ❌ Tipo da exceção: {ex.GetType().Name}");
                throw;
            }

            
            if (product.ImagesID != null && product.ImagesID.Any() && 
                product.ImagesID.Any(img => !string.IsNullOrEmpty(img)))
            {
                
                // 🔧 CORREÇÃO: Verificar se há imagens que são GUIDs (novas imagens temporárias)
                // As imagens temporárias são GUIDs, não começam com "temp_"
                var tempImages = product.ImagesID.Where(img => !string.IsNullOrEmpty(img) && 
                    (img.StartsWith("temp_") || IsGuid(img))).ToList();
                
                if (tempImages.Any())
                {
                    
                    ICollection<ImageProduct> imageProducts = productDb.ColProdutoImagem;
                    string oldImage = productDb.ColProdutoImagem.First().Nome;
                    string oldThumb = productDb.ProdutoThumbnail.Nome;

                    foreach (var image in imageProducts)
                    {
                        foreach (var matchingImage in tempImages)
                        {
                            string arquivoNome = MoveImageFile(matchingImage);
                            
                            image.Nome = arquivoNome;
                            image.Arquivo = arquivoNome;
                            image.DataEdicao = DateTime.Now;
                            image.UsuarioIDEdicao = UserID;

                            await _cardapioUnitOfWork.ProdutoImagemRepo.UpdateAsync(image);

                            var thumbnailProduct = await _cardapioUnitOfWork.ProdutoThumbnailRepo.GetThumbnailByID(productDb.ProdutoThumbnailID ?? 0);

                            thumbnailProduct.Nome = arquivoNome;
                            thumbnailProduct.Arquivo = arquivoNome;
                            thumbnailProduct.DataEdicao = DateTime.Now;
                            thumbnailProduct.UsuarioIDEdicao = UserID;

                            // 🔧 CORREÇÃO: Usar o nome do arquivo final, não o GUID original
                            CreateThumbFile(arquivoNome, UserID);

                            await _cardapioUnitOfWork.ProdutoThumbnailRepo.UpdateAsync(thumbnailProduct);

                            if (oldImage != arquivoNome && oldImage != "Default.png")
                            {
                                DeleteImageFile(oldImage, oldThumb);
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[UpdateProducts] ✅ Nenhuma imagem nova - mantendo imagem atual");
                }
            }
            else
            {
                Console.WriteLine($"[UpdateProducts] ⚠️ Nenhuma imagem fornecida - definindo como Default.png");
                
                // 🔧 CORREÇÃO: Se não há imagens, definir como Default.png
                ICollection<ImageProduct> imageProducts = productDb.ColProdutoImagem;
                string oldImage = productDb.ColProdutoImagem.First().Nome;
                string oldThumb = productDb.ProdutoThumbnail.Nome;

                foreach (var image in imageProducts)
                {
                    image.Nome = "Default.png";
                    image.Arquivo = "Default.png";
                    image.DataEdicao = DateTime.Now;
                    image.UsuarioIDEdicao = UserID;

                    await _cardapioUnitOfWork.ProdutoImagemRepo.UpdateAsync(image);

                    var thumbnailProduct = await _cardapioUnitOfWork.ProdutoThumbnailRepo.GetThumbnailByID(productDb.ProdutoThumbnailID ?? 0);

                    thumbnailProduct.Nome = "Default.png";
                    thumbnailProduct.Arquivo = "Default.png";
                    thumbnailProduct.DataEdicao = DateTime.Now;
                    thumbnailProduct.UsuarioIDEdicao = UserID;

                    await _cardapioUnitOfWork.ProdutoThumbnailRepo.UpdateAsync(thumbnailProduct);

                    // Remover imagem antiga se não for Default.png
                    if (oldImage != "Default.png" && !string.IsNullOrEmpty(oldImage))
                    {
                        DeleteImageFile(oldImage, oldThumb);
                    }
                }
            }

            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> DeactivateProduct(ClaimsPrincipal User, int productID, int empresaID)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCompanyIdIsPresent(usuario.EmpresaID);

            Infra.Model.Product productDb = await GetProductByID(productID, empresaID);
            _validator.VerifyProductIdIsPresent(usuario.EmpresaID);

            await DeactivateProductDetails(productDb, UserID);
            await DeactivateImagesAndThumbnailsAsync(productID, UserID);

            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        private async Task DeactivateProductDetails(Infra.Model.Product product, int usuarioID)
        {
            product.Ativo = false;
            product.Excluido = true;
            product.DataEdicao = DateTime.UtcNow;
            product.UsuarioIDEdicao = usuarioID;

            await _cardapioUnitOfWork.ProdutoRepo.UpdateAsync(product);
        }

        private async Task DeactivateImagesAndThumbnailsAsync(int productId, int usuarioID)
        {
            List<ImageProduct> images = _appDbContext.ProdutoImagem
                .Where(img => img.ProdutoID == productId && img.Ativo)
                .ToList();

            foreach (var image in images)
            {
                image.Ativo = false;
                image.DataEdicao = DateTime.UtcNow;
                image.UsuarioIDEdicao = usuarioID;

                await _cardapioUnitOfWork.ProdutoImagemRepo.UpdateAsync(image);

                if (image.Arquivo != "Default.png")
                {
                    DeleteFileIfExists(Path.Combine(_appPathsService.GetProductImagesDirectory(), image.Arquivo));
                }
            }

            ThumbnailProduct? thumbnail = _appDbContext.ProdutoThumbnail
                .Where(t => t.ID == GetProductThumbnailID(productId) && t.Ativo)
                .FirstOrDefault();

            if (thumbnail != null)
            {
                thumbnail.Ativo = false;
                thumbnail.DataEdicao = DateTime.UtcNow;
                thumbnail.UsuarioIDEdicao = usuarioID;

                await _cardapioUnitOfWork.ProdutoThumbnailRepo.UpdateAsync(thumbnail);

                if (thumbnail.Arquivo != "Default.png")
                {
                    DeleteFileIfExists(Path.Combine(_appPathsService.GetThumbnailsDirectory(), thumbnail.Arquivo));
                }
            }
        }

        private void DeleteFileIfExists(string filePath)
        {
            var file = new FileInfo(filePath);
            if (file.Exists)
            {
                file.Delete();
            }
        }

        public async Task<ActionResult> DeletePromotion(ClaimsPrincipal User, int promocaoId)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(UserID);


            try 
            {
                var produtoPromocaoHorarioService = _serviceProvider.GetService<ProdutoPromocaoHorarioService>();
                if (produtoPromocaoHorarioService == null)
                {
                    throw new ErrorResponse("Serviço de promoções não disponível.", 500);
                }

                var resultado = await produtoPromocaoHorarioService.DesativarPromocaoAsync(promocaoId, UserID);
                
                if (resultado)
                {
                    // Commit da transação
                    await _cardapioUnitOfWork.Commit();
                    return new OkResult();
                }
                else
                {
                    throw new ErrorResponse("Promoção não encontrada ou já inativa.", 404);
                }
            }
            catch (Exception ex)
            {
                throw new ErrorResponse($"Erro ao excluir promoção: {ex.Message}", 500);
            }
        }

        public async Task<ActionResult> ExportProduct(ClaimsPrincipal User, ExportarProdutosDTO exportDto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyCategoryIdIsPresent(userId);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(userId);
            _validator.VerifyCompanyIdIsPresent(usuario.EmpresaID);

            var produtoQueJaFoiAdicionado = new Dictionary<int, int>();
            var grupoQueJaFoiAdicionado = new Dictionary<int, int>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (exportDto.ImportCheck)
            {
                foreach (var product in exportDto.Produtos)
                {
                    foreach (var oldGroupID in product.GruposID)
                    {
                        if (!grupoQueJaFoiAdicionado.ContainsKey(oldGroupID.ID))
                        {
                            var newGroup = await GenerateAdditionals(oldGroupID, exportDto);
                            grupoQueJaFoiAdicionado[oldGroupID.ID] = newGroup.ID;
                        }
                    }
                }
            }

            foreach (var product in exportDto.Produtos)
            {
                if (!produtoQueJaFoiAdicionado.ContainsKey(product.ProdutoId))
                {
                    var (produtoDB, categoriaDB) = await GetProdutoComCategoriaAsync(product.ProdutoId, exportDto.EmpresaIdFrom);

                    var categoryAlreadyExistingCompany = GetCategoryByNameAsync(categoriaDB.Nome, exportDto.EmpresaIdTo);
                    Infra.Model.Product productAlreadyExistingCompany = null;
                    Infra.Model.Category newCategory = null;

                    if (categoryAlreadyExistingCompany == null)
                    {
                        newCategory = await CreateCategoryIfNotExist(categoriaDB, exportDto.EmpresaIdTo, usuario);
                        productAlreadyExistingCompany = GetProductByNameAsync(produtoDB.Nome, exportDto.EmpresaIdTo, newCategory.ID);
                    }
                    else
                    {
                        productAlreadyExistingCompany = GetProductByNameAsync(produtoDB.Nome, exportDto.EmpresaIdTo, categoryAlreadyExistingCompany.ID);
                    }

                    if (productAlreadyExistingCompany == null)
                    {
                        var productModel = GenerateNewExportModel(produtoDB, categoryAlreadyExistingCompany ?? newCategory, product);
                        var newProduct = CreateProductModel(productModel, usuario.ID, exportDto.EmpresaIdTo, categoryAlreadyExistingCompany ?? newCategory);

                        if (produtoDB.ProdutoThumbnail.Arquivo.Contains("Default.png") ||
                            produtoDB.ProdutoThumbnail.Nome.Contains("Default"))
                        {
                            await GenerateNewImageDefaultExport(newProduct, productModel, usuario);
                        }
                        else
                        {
                            string newImageName = Guid.NewGuid() + ".jpg";
                            var thumbnail = await CopyThumbImage(produtoDB.ProdutoThumbnail.Nome, newImageName, usuario.ID);
                            newProduct.ProdutoThumbnailID = thumbnail.ID;

                            await _cardapioUnitOfWork.ProdutoRepo.AddAsync(newProduct);
                            await _cardapioUnitOfWork.Commit();

                            await CopyImage(produtoDB.ProdutoThumbnail.Nome, newImageName, usuario.ID, newProduct.ID);
                        }

                        produtoQueJaFoiAdicionado[product.ProdutoId] = newProduct.ID;
                    }
                }

                foreach (var oldGroupID in product.GruposID)
                {
                    var newGroupId = grupoQueJaFoiAdicionado[oldGroupID.ID];
                    if (produtoQueJaFoiAdicionado.Any() && produtoQueJaFoiAdicionado[product.ProdutoId] != 0 && newGroupId != 0)
                    {
                        var produtoGrupoAdicional = new ProdutoGrupoAdicional
                        {
                            GrupoAdicionalID = newGroupId,
                            ProdutoID = produtoQueJaFoiAdicionado[product.ProdutoId],
                            Ativo = true,
                        };

                        await _cardapioUnitOfWork.ProdutoGrupoAdicionalRepo.AddAsync(produtoGrupoAdicional);
                    }
                }

                await _cardapioUnitOfWork.Commit();
            }

            stopwatch.Stop();

            return new OkResult();
        }

        private async Task<(Infra.Model.Product produto, Infra.Model.Category categoria)> GetProdutoComCategoriaAsync(int produtoId, int empresaId)
        {
            var produtoComCategoria = await _appDbContext.Produto
                .Where(p => p.ID == produtoId && p.EmpresaID == empresaId)
                .Include(p => p.Categoria).Where(p => p.Ativo)
                .Include(p => p.ProdutoThumbnail)
                .FirstOrDefaultAsync();

            if (produtoComCategoria != null)
            {
                return (produtoComCategoria, produtoComCategoria.Categoria);
            }

            return (null, null);
        }

        private async Task<ThumbnailProduct> CopyThumbImage(string oldImageID, string newImageID, int usuarioID)
        {
            string oldFilePath = Path.Combine(_appPathsService.GetThumbnailsDirectory(), oldImageID);
            string newFilePath = Path.Combine(_appPathsService.GetThumbnailsDirectory(), newImageID);


            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
            }

            ThumbnailProduct thumbnail = new ThumbnailProduct()
            {
                Nome = newImageID,
                Arquivo = newImageID,
                Altura = 0,
                Largura = 0,
                Tamanho = 0,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
                UsuarioIDCadastro = usuarioID,
                UsuarioIDEdicao = usuarioID,
                Ativo = true,
            };

            await _cardapioUnitOfWork.ProdutoThumbnailRepo.AddAsync(thumbnail);
            await _cardapioUnitOfWork.Commit();

            return thumbnail;
        }

        private async Task<ImageProduct> CopyImage(string oldImageID, string newImageID, int usuarioID, int productID)
        {
            string oldFilePath = Path.Combine(_appPathsService.GetProductImagesDirectory(), oldImageID);
            string newFilePath = Path.Combine(_appPathsService.GetProductImagesDirectory(), newImageID);


            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
            }

            ImageProduct image = new ImageProduct()
            {
                Nome = newImageID,
                Arquivo = newImageID,
                Altura = 0,
                Largura = 0,
                Tamanho = 0,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
                UsuarioIDCadastro = usuarioID,
                UsuarioIDEdicao = usuarioID,
                ProdutoID = productID,
                Ativo = true,
            };

            await _cardapioUnitOfWork.ProdutoImagemRepo.AddAsync(image);
            await _cardapioUnitOfWork.Commit();

            return image;
        }

        private void CopyImageAdditional(string oldImageID, string newImageID)
        {
            string oldFilePath = Path.Combine(_appPathsService.GetAdditionalImagesDirectory(), oldImageID);
            string newFilePath = Path.Combine(_appPathsService.GetAdditionalImagesDirectory(), newImageID);


            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
            }
        }

        private async Task GenerateNewImageDefaultExport(Infra.Model.Product newProduct, ProdutoAddDTO productModel, UsuarioGetDTO usuario)
        {
            ThumbnailProduct thumbProduct = new ThumbnailProduct()
            {
                Nome = "Default.png",
                Arquivo = "Default.png",
                Ativo = true,
                Altura = 0,
                Largura = 0,
                Tamanho = 0,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
                UsuarioIDCadastro = usuario.ID,
                UsuarioIDEdicao = usuario.ID,
            };

            await _cardapioUnitOfWork.ProdutoThumbnailRepo.AddAsync(thumbProduct);
            await _cardapioUnitOfWork.Commit();

            newProduct.ProdutoThumbnailID = thumbProduct.ID;

            await _cardapioUnitOfWork.ProdutoRepo.AddAsync(newProduct);
            await _cardapioUnitOfWork.Commit();

            ImageProduct imageProduct = new ImageProduct()
            {
                Nome = "Default.png",
                Arquivo = "Default.png",
                Ativo = true,
                Altura = 0,
                Largura = 0,
                Tamanho = 0,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
                UsuarioIDCadastro = usuario.ID,
                UsuarioIDEdicao = usuario.ID,
                ProdutoID = newProduct.ID
            };

            await _cardapioUnitOfWork.ProdutoImagemRepo.AddAsync(imageProduct);
        }

        private async Task<GrupoAdicional> GenerateAdditionals(GrupoAdicionalGetDTO oldGroup, ExportarProdutosDTO exportDTO)
        {
            GrupoAdicional group = await _cardapioUnitOfWork.GrupoAdicionalRepo.GetGrupoAdicionalAsyncByIdAndEmpresaID(oldGroup.ID, exportDTO.EmpresaIdFrom);
            GrupoAdicional existentGroup = await _cardapioUnitOfWork.GrupoAdicionalRepo.CompararGrupoEBuscarOuCriarNovo(exportDTO.EmpresaIdTo, group);

            if (existentGroup != null && existentGroup.ID == 0)
            {
                GrupoAdicional newGroup = new GrupoAdicional()
                {
                    Nome = existentGroup.Nome ?? group.Nome,
                    Minimo = group.Minimo,
                    Maximo = group.Maximo,
                    Ativo = true,
                    DataCadastro = new DateTime(),
                    DataEdicao = new DateTime(),
                    TipoID = group.TipoID,
                    UsuarioIDCadastro = group.UsuarioIDCadastro,
                    UsuarioIDEdicao = group.UsuarioIDEdicao,
                    EmpresaID = exportDTO.EmpresaIdTo,
                };

                await _cardapioUnitOfWork.GrupoAdicionalRepo.AddAsync(newGroup);
                await _cardapioUnitOfWork.Commit();

                foreach (var item in group.Produtos)
                {
                    GrupoAdicionalItem newItem = new GrupoAdicionalItem()
                    {
                        Nome = item.Nome,
                        Preco = item.Preco,
                        UsuarioIDCadastro = group.UsuarioIDCadastro,
                        UsuarioIDEdicao = group.UsuarioIDEdicao,
                        DataCadastro = new DateTime(),
                        DataEdicao = new DateTime(),
                        Ativo = true,
                        GrupoAdicionalID = newGroup.ID,
                        EmpresaID = exportDTO.EmpresaIdTo,
                    };

                    await _cardapioUnitOfWork.GrupoAdicionalItemRepo.AddAsync(newItem);
                    await _cardapioUnitOfWork.Commit();

                    foreach (var image in item.ColImagemAdicional)
                    {
                        string uuid = "default.png";

                        if (image.Nome != "Default.png")
                        {
                            uuid = Guid.NewGuid().ToString();
                            CopyImageAdditional(image.Nome, uuid);
                        }

                        GrupoAdicionalItemImagem grupoAdicionalItemImagem = new GrupoAdicionalItemImagem()
                        {
                            Nome = image.Nome,
                            Largura = image.Largura,
                            Altura = image.Altura,
                            Arquivo = image.Arquivo,
                            Ativo = true,
                            GrupoAdicionalItemID = newItem.ID,
                            DataCadastro = new DateTime(),
                            DataEdicao = new DateTime(),
                            UsuarioIDCadastro = group.UsuarioIDCadastro,
                            UsuarioIDEdicao = group.UsuarioIDEdicao,
                        };

                        await _cardapioUnitOfWork.GrupoAdicionalItemImagemRepo.AddAsync(grupoAdicionalItemImagem);
                        await _cardapioUnitOfWork.Commit();
                    }
                }

                return newGroup;
            }

            return existentGroup;
        }

        private ProdutoAddDTO GenerateNewExportModel(Infra.Model.Product produtoDB, Infra.Model.Category categoriaDb, ProdutoExportDTO product)
        {
            ProdutoAddDTO produtoModel = new ProdutoAddDTO
            {
                Descricao = produtoDB.Descricao,
                Destaque = produtoDB.Destaque,
                Nome = produtoDB.Nome,
                Preco = product.Preco,
                Promocao = produtoDB.Promocao,
                QTDPessoa = produtoDB.QTDPessoa,
                PrecoPromocional = produtoDB.PrecoPromocional,
                FimPromocao = produtoDB.FimPromocao,
                Ativo = produtoDB.Ativo,
                CategoriaID = categoriaDb.ID,
            };

            return produtoModel;
        }

        private async Task<Infra.Model.Category> CreateCategoryIfNotExist(Infra.Model.Category categoriaDB, int empresaId, UsuarioGetDTO usuario)
        {
            IEnumerable<CategoriaGetDTO> categoriasExistentes = await _cardapioUnitOfWork.CategoriaRepo
                .GetCategoriaByEmpresaIDAsyncNotAuthenticated(empresaId);

            int? ordemCorrigida = categoriasExistentes
                .Select(c => c.Ordem)
                .OrderBy(o => o)
                .FirstOrDefault(ordem => ordem > 1);

            int? novaOrdem = categoriaDB.Ordem > 0 && !categoriasExistentes.Any(c => c.Ordem == categoriaDB.Ordem)
                ? categoriaDB.Ordem ?? 0
                : ordemCorrigida + 1;

            Infra.Model.Category newCategory = new Infra.Model.Category
            {
                Ativo = true,
                EmpresaID = empresaId,
                UsuarioIDCadastro = usuario.ID,
                UsuarioIDEdicao = usuario.ID,
                Nome = categoriaDB.Nome,
                Ordem = novaOrdem,
                BackgroundCor = categoriaDB.BackgroundCor,
                DataCadastro = DateTime.UtcNow,
                DataEdicao = DateTime.UtcNow,
            };

            await _cardapioUnitOfWork.CategoriaRepo.AddAsync(newCategory);
            return newCategory;
        }

        private Infra.Model.Product GetProductByNameAsync(string nome, int empresaID, int categoriaID)
        {
            return _appDbContext.Produto
                .Where(c => c.Nome == nome && c.EmpresaID == empresaID && c.Ativo && c.CategoriaID == categoriaID && !c.Excluido)
                .FirstOrDefault();
        }

        private Infra.Model.Category GetCategoryByNameAsync(string nome, int empresaID)
        {
            return _appDbContext.Categoria
                .Where(c => c.Nome == nome && c.EmpresaID == empresaID && c.Ativo)
                .FirstOrDefault();
        }

        public async Task<ActionResult> VerifyProductAlreadyExists(int UserID, Client.Dto.VerifyProductDTO verifyProductDTO, int empresaID)
        {
            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);

            var nomeProduto = verifyProductDTO.ProdutoNome?.Trim().ToLower();
            IEnumerable<Infra.Model.Product> queryProdutos = _appDbContext.Produto
                .Where(c => c.Nome.Trim().ToLower() == nomeProduto
                            && c.Ativo
                            && c.EmpresaID == empresaID
                            && c.CategoriaID == verifyProductDTO.CategoriaID
                            && !c.Excluido)
                .ToList();

            if (verifyProductDTO.ID != null)
            {
                queryProdutos = queryProdutos.Where(c => c.ID != verifyProductDTO.ID);
            }

            foreach (Infra.Model.Product product in queryProdutos)
            {
                _validator.VerifyProductAlreadyExists(product);
            }

            return new OkResult();
        }

        private int GetProductThumbnailID(int productId)
        {
            return _appDbContext.Produto
                .Where(p => p.ID == productId)
                .Select(p => p.ProdutoThumbnailID)
                .FirstOrDefault() ?? 0;
        }

        private async Task<Infra.Model.Product> GetProductByID(int productID, int empresaID)
        {
            return await _cardapioUnitOfWork.ProdutoRepo.GetByIDAsync(productID, empresaID);
        }

        private Infra.Model.Category GetCategoryAsync(int? categoriaID)
        {
            return _appDbContext.Categoria
                .Where(c => c.ID == categoriaID && c.Ativo)
                .FirstOrDefault();
        }

        private Infra.Model.Product CreateProductModel(ProdutoAddDTO product, int usuarioID, int empresaID, Infra.Model.Category category)
        {
            return new Infra.Model.Product
            {
                Nome = product.Nome,
                CategoriaID = product.CategoriaID,
                EmpresaID = empresaID,
                DataCadastro = DateTime.UtcNow,
                DataEdicao = DateTime.UtcNow,
                UsuarioIDCadastro = usuarioID,
                UsuarioIDEdicao = usuarioID,
                Categoria = category,
                QTDPessoa = product.QTDPessoa,
                Descricao = product.Descricao,
                Promocao = product.Promocao,
                PrecoPromocional = product.Promocao ? product.PrecoPromocional : null,
                FimPromocao = product.Promocao ? product.FimPromocao : null,
                Destaque = product.Destaque,
                Excluido = false,
                Ativo = product.Ativo,
                Preco = product.Preco
            };
        }

        private Infra.Model.Product CreateProductUpdateModel(ProdutoAddDTO product, Infra.Model.Product productDB, int usuarioID, Infra.Model.Category category)
        {
            productDB.Nome = product.Nome;
            productDB.CategoriaID = category.ID;
            productDB.Preco = product.Preco;
            productDB.UsuarioIDEdicao = usuarioID;
            productDB.QTDPessoa = product.QTDPessoa;
            productDB.Descricao = product.Descricao ?? "";
            productDB.Destaque = product.Destaque;
            productDB.Ativo = product.Ativo;
            productDB.Promocao = product.Promocao;
            productDB.PrecoPromocional = product.Promocao ? product.PrecoPromocional : null;
            productDB.FimPromocao = product.Promocao ? product.FimPromocao : null;

            return productDB;
        }

        private async Task<Infra.Model.Product> AddProductAsync(Infra.Model.Product productModel)
        {
            await _cardapioUnitOfWork.ProdutoRepo.AddAsync(productModel);
            await _cardapioUnitOfWork.Commit();

            return productModel;
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
    }
}
