using Cardápio.Dto;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SizeSharp = SixLabors.ImageSharp.Size;
using ImageSharp = SixLabors.ImageSharp.Image;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cardápio.Client.Infra.Crypto;
using Cardápio.Infra.Helpers;
using Cardápio.Domain.QrCode;
using Cardápio.Infra.Model;
using Cardápio.Infra.Data;

namespace Cardápio.Domain.Enterprise
{
    public class EnterpriseService
    {
        private readonly string basePath;

        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;
        private readonly QrCodeService _qrCodeService;
        private readonly Crypto _crypto;

        public EnterpriseService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext,
                                Validator validator, QrCodeService qrCodeService, Crypto crypto)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
            _qrCodeService = qrCodeService;
            _crypto = crypto;
            basePath = GetProjectBasePath();
        }

        private string GetProjectBasePath()
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;

            // Subir até encontrar o diretório do projeto (onde está o arquivo .csproj)
            var directory = new DirectoryInfo(currentPath);
            while (directory != null && !directory.GetFiles("*.csproj").Any())
            {
                directory = directory.Parent;
            }

            string projectBasePath = directory?.FullName ?? currentPath;

            // Garantir que os diretórios de imagem existem
            EnsureImageDirectoriesExist(projectBasePath);

            return projectBasePath;
        }

        private void EnsureImageDirectoriesExist(string projectPath)
        {
            try
            {
                string imagensPath = Path.Combine(projectPath, "imagens");
                string bannerPath = Path.Combine(imagensPath, "Banner");
                string logoPath = Path.Combine(imagensPath, "Logo");

                if (!Directory.Exists(imagensPath))
                {
                    Directory.CreateDirectory(imagensPath);
                }

                if (!Directory.Exists(bannerPath))
                {
                    Directory.CreateDirectory(bannerPath);
                }

                if (!Directory.Exists(logoPath))
                {
                    Directory.CreateDirectory(logoPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EnterpriseService] Erro ao criar diretórios: {ex.Message}");
            }
        }

        public async Task<ActionResult> GetEnterprises(ClaimsPrincipal User)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int UserID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            int grupoID = await GetGroupIDAsync(usuario.ID);
            UserGroup groupUser = await GetGroupUserIDAsync(UserID);

            IEnumerable<EmpresaGetDTO> response = await GetEnterprisesIDAsync(usuario.ID, grupoID, usuario.UsuarioTipoID ?? 0, usuario.EmpresaID ?? 0);

            return new OkObjectResult(response);
        }

        public async Task<ActionResult> GetEnterprisesByUserID(ClaimsPrincipal User, int usuarioEnterprises)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int UserID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            int grupoID = await GetGroupIDAsync(usuarioEnterprises);
            UserGroup groupUser = await GetGroupUserIDAsync(UserID);

            IEnumerable<EmpresaGetDTO> response = await _cardapioUnitOfWork.EmpresaRepo.GetEnterpriseByGrupoID(grupoID);

            return new OkObjectResult(response);
        }

        public async Task<ActionResult> GetEnterprisesByGroupID(ClaimsPrincipal User, int groupID)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int UserID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            GrupoGetDTO grupo = await GetGroupAsyncById(groupID);
            UserGroup groupUser = await GetGroupUserIDAsync(UserID);

            IEnumerable<EmpresaGetDTO> response = await _cardapioUnitOfWork.EmpresaRepo.GetEnterpriseByGrupoID(grupo.ID);

            return new OkObjectResult(response);
        }

        public async Task<ActionResult> GetEnterpriseByName(string empresaNome)
        {
            EmpresaGetDTO response = await _cardapioUnitOfWork.EmpresaRepo.GetEmpresaByNameAsync(empresaNome);

            return new OkObjectResult(response);
        }

        public async Task<ActionResult> GetEnterpriseByID(ClaimsPrincipal User, int empresaID)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int UserID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);
            _validator.VerifyCategoryIdIsPresent(usuario.EmpresaID);

            EmpresaGetDTO response = await _cardapioUnitOfWork.EmpresaRepo.GetEmpresaByIDAsync(empresaID);

            return new OkObjectResult(response);
        }

        public async Task<ActionResult> GetCep(string cep)
        {
            List<EnderecoDto> endereco = await _appDbContext.ExecutarSPEnderecoAsync(cep);

            return new OkObjectResult(endereco);
        }

        public async Task<ActionResult> AddCompanyImages(ClaimsPrincipal User, IFormFile? banner, IFormFile? logo)
        {
            string bannerFileName = "";
            string logoFileName = "";

            if (banner == null && logo == null)
            {
                return new OkObjectResult("Nenhum arquivo selecionado");
            }

            if (banner != null)
            {
                bannerFileName = $"{Guid.NewGuid()}.png";
                string filePath = Path.Combine(basePath, "imagens", "Banner", bannerFileName);

                using (var bannerStream = new MemoryStream())
                {
                    await banner.CopyToAsync(bannerStream);
                    bannerStream.Position = 0;

                    byte[] buffer = new byte[8];
                    bannerStream.Read(buffer, 0, buffer.Length);
                    bannerStream.Position = 0;

                    try
                    {
                        using (var bannerImage = ImageSharp.Load(bannerStream))
                        {
                            if (bannerImage.Width > 1340)
                            {
                                bannerImage.Mutate(x => x.Resize(new ResizeOptions
                                {
                                    Mode = ResizeMode.Max,
                                    Size = new SizeSharp(1340, 0)
                                }));
                            }

                            using (var outputStream = new MemoryStream())
                            {
                                bannerImage.SaveAsPng(outputStream);
                                outputStream.Position = 0;

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await outputStream.CopyToAsync(fileStream);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return new BadRequestObjectResult("Erro ao processar a imagem.");
                    }
                }
            }

            if (logo != null)
            {
                logoFileName = $"{Guid.NewGuid()}.png";
                string filePath = Path.Combine(basePath, "imagens", "Logo", logoFileName);

                using (var logoStream = new MemoryStream())
                {
                    await logo.CopyToAsync(logoStream);
                    logoStream.Position = 0;

                    try
                    {
                        using (var logoImage = ImageSharp.Load(logoStream))
                        {
                            logoImage.Mutate(x => x.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Crop,
                                Size = new SizeSharp(130, 130)
                            }));

                            using (var outputStream = new MemoryStream())
                            {
                                logoImage.SaveAsPng(outputStream);
                                outputStream.Position = 0;

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await outputStream.CopyToAsync(fileStream);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return new BadRequestObjectResult("Erro ao processar o logo.");
                    }
                }
            }

            
            // Verificar se os arquivos foram realmente salvos
            if (!string.IsNullOrEmpty(bannerFileName))
            {
                string bannerPath = Path.Combine(basePath, "imagens", "Banner", bannerFileName);
            }
            
            if (!string.IsNullOrEmpty(logoFileName))
            {
                string logoPath = Path.Combine(basePath, "imagens", "Logo", logoFileName);
            }

            return new OkObjectResult(new { FileBanner = bannerFileName, FileLogo = logoFileName });
        }

        public async Task<ActionResult> AddCompany(ClaimsPrincipal User, EmpresaAddDTO empresaAddDTO)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int UserID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(UserID);

            UsuarioGetDTO usuario = await GetEmpresaIDAsync(UserID);

            int grupoID = await GetGroupIDAsync(usuario.ID);
            GrupoGetDTO grupo = await GetGroupAsyncById(grupoID);

            Company company = new Company();
            company.CEP = empresaAddDTO.CEP;
            company.UsuarioIDCadastro = UserID;
            company.UsuarioIDEdicao = UserID;
            company.RazaoSocial = empresaAddDTO.RazaoSocial;
            company.CNPJ = empresaAddDTO.CNPJ;
            company.Ativo = true;
            company.Celular = empresaAddDTO.Celular;
            company.Telefone = empresaAddDTO.Telefone;
            company.EstadoID = empresaAddDTO.EstadoID;
            company.CidadeID = empresaAddDTO.CidadeID;
            company.QRCode = empresaAddDTO.QRCode;
            company.DiasDemo = 10;
            company.Nome = empresaAddDTO.Nome;
            company.GrupoID = grupoID;
            company.AtenderWhatsapp = empresaAddDTO.AtenderWhatsapp;

            if (grupo.GrupoTipoID == 1)
            {
                company.EmpresaTipoID =EmpresaTipoHelper.Demo;
            }
            else
            {
                company.EmpresaTipoID = empresaAddDTO.EmpresaTipoID == 0 ? EmpresaTipoHelper.CLientes : company.EmpresaTipoID;
            }

            await _cardapioUnitOfWork.EmpresaRepo.AddAsync(company);
            await _cardapioUnitOfWork.Commit();

            company.QRCode = await _qrCodeService.SaveQrCode(company.Nome);

            UsuarioEmpresa usuarioEmpresa = new UsuarioEmpresa
            {
                EmpresaID = company.ID,
                UsuarioID = usuario.ID
            };

            await _cardapioUnitOfWork.UsuarioEmpresaRepo.AddAsync(usuarioEmpresa);
            await _cardapioUnitOfWork.Commit();

            await ProcessAndAddImagesAsync(
                company,
                empresaAddDTO.ColLogoImagem?.Where(x => x != null).Cast<Cardápio.Dto.ImageFileAddDTO>() ?? Enumerable.Empty<Cardápio.Dto.ImageFileAddDTO>(),
                empresaAddDTO.ColBannerImagem?.Where(x => x != null).Cast<Cardápio.Dto.ImageFileAddDTO>() ?? Enumerable.Empty<Cardápio.Dto.ImageFileAddDTO>(),
                UserID);

            return new OkResult();
        }

        public async Task<ActionResult> AddCompanyInGroupID(ClaimsPrincipal User, EmpresaAddDTO empresaAddDTO, int groupID)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int UserID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(UserID);

            GrupoGetDTO grupo = await GetGroupAsyncById(groupID);
            if (grupo == null)
                throw new ErrorResponse($"Grupo {groupID} não encontrado.", 404);

            Infra.Model.User usuarioAdmin = await _cardapioUnitOfWork.UsuarioRepo.GetUsuarioAdminByGrupoId(grupo.ID);
            // <-- removido o throw, usa UserID como fallback
            int adminID = usuarioAdmin?.ID ?? UserID;

            Company company = new Company();
            company.CEP = empresaAddDTO.CEP;
            company.UsuarioIDCadastro = UserID;
            company.UsuarioIDEdicao = UserID;
            company.RazaoSocial = empresaAddDTO.RazaoSocial;
            company.CNPJ = empresaAddDTO.CNPJ;
            company.Ativo = true;
            company.Celular = empresaAddDTO.Celular;
            company.Telefone = empresaAddDTO.Telefone;
            company.EstadoID = empresaAddDTO.EstadoID;
            company.CidadeID = empresaAddDTO.CidadeID;
            company.QRCode = empresaAddDTO.QRCode;
            company.Nome = empresaAddDTO.Nome;
            company.GrupoID = grupo.ID;
            company.EmpresaTipoID = empresaAddDTO.EmpresaTipoID;
            company.AtenderWhatsapp = empresaAddDTO.AtenderWhatsapp;
            company.DiasDemo = 10;
            company.DataCadastro = DateTime.UtcNow;
            company.DataEdicao = DateTime.UtcNow;
            company.Excluido = false;

            await _cardapioUnitOfWork.EmpresaRepo.AddAsync(company);
            await _cardapioUnitOfWork.Commit();

            company.QRCode = await _qrCodeService.SaveQrCode(company.Nome);

            var logoImagens = empresaAddDTO.ColLogoImagem?.Where(x => x != null)
                .Cast<Cardápio.Dto.ImageFileAddDTO>() ?? Enumerable.Empty<Cardápio.Dto.ImageFileAddDTO>();
            var bannerImagens = empresaAddDTO.ColBannerImagem?.Where(x => x != null)
                .Cast<Cardápio.Dto.ImageFileAddDTO>() ?? Enumerable.Empty<Cardápio.Dto.ImageFileAddDTO>();

            if (!logoImagens.Any())
            {
                await _cardapioUnitOfWork.LogoRepo.AddAsync(new Logo
                {
                    Nome = "logo_default",
                    Arquivo = "logo_default.png",
                    Altura = 80,
                    Largura = 80,
                    Ativo = true,
                    EmpresaID = company.ID,
                    Tamanho = 0,
                    UsuarioIDCadastro = UserID,
                    UsuarioIDEdicao = UserID,
                    DataCadastro = DateTime.UtcNow,
                    DataEdicao = DateTime.UtcNow,
                });
            }

            if (!bannerImagens.Any())
            {
                await _cardapioUnitOfWork.BannerRepo.AddAsync(new Banner
                {
                    Nome = "banner_default",
                    Arquivo = "banner_default.png",
                    Altura = 1300,
                    Largura = 600,
                    Ativo = true,
                    EmpresaID = company.ID,
                    Tamanho = 0,
                    UsuarioIDCadastro = UserID,
                    UsuarioIDEdicao = UserID,
                    DataCadastro = DateTime.UtcNow,
                    DataEdicao = DateTime.UtcNow,
                });
            }

            await _cardapioUnitOfWork.Commit();

            await ProcessAndAddImagesAsync(company, logoImagens, bannerImagens, UserID);

            await _cardapioUnitOfWork.UsuarioEmpresaRepo.AddAsync(new UsuarioEmpresa
            {
                EmpresaID = company.ID,
                UsuarioID = adminID // <-- usa adminID que já tem o fallback
            });
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> EditCompany(ClaimsPrincipal user, int empresaID, EmpresaUpdateDTO empresaUpdateDTO)
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int userID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(userID);

            Company existingCompany = await _cardapioUnitOfWork.EmpresaRepo.GetByIDAsync(empresaID);
            _validator.VerifyCategoryIdIsPresent(existingCompany.ID);

            if (existingCompany != null && existingCompany.Nome != empresaUpdateDTO.Nome)
            {
                existingCompany.QRCode = await _qrCodeService.SaveQrCode(empresaUpdateDTO.Nome);
            }

            existingCompany.CEP = empresaUpdateDTO?.CEP?.Trim() ?? string.Empty;
            existingCompany.RazaoSocial = empresaUpdateDTO?.RazaoSocial?.Trim() ?? string.Empty;
            existingCompany.CNPJ = empresaUpdateDTO?.CNPJ?.Trim() ?? string.Empty;
            existingCompany.Celular = empresaUpdateDTO?.Celular?.Trim() ?? string.Empty;
            existingCompany.Telefone = empresaUpdateDTO?.Telefone?.Trim() ?? string.Empty;
            existingCompany.EstadoID = empresaUpdateDTO?.EstadoID ?? string.Empty;
            existingCompany.CidadeID = empresaUpdateDTO?.CidadeID ?? 0;
            existingCompany.Nome = empresaUpdateDTO?.Nome ?? string.Empty;
            existingCompany.UsuarioIDEdicao = userID;
            existingCompany.Ativo = empresaUpdateDTO.Ativo;
            existingCompany.EmpresaTipoID = empresaUpdateDTO.EmpresaTipoID == 0 ? existingCompany.EmpresaTipoID : empresaUpdateDTO.EmpresaTipoID;
            existingCompany.AtenderWhatsapp = empresaUpdateDTO.AtenderWhatsapp;

            string? oldPathBanner = null;
            string? oldPathLogo = null;

            if (existingCompany.ColBanner != null && existingCompany.ColBanner.Any())
            {
                oldPathBanner = existingCompany.ColBanner.First().Arquivo;
            }

            if (existingCompany.ColLogo != null && existingCompany.ColLogo.Any())
            {
                oldPathLogo = existingCompany.ColLogo.First().Arquivo;
            }

            await _cardapioUnitOfWork.EmpresaRepo.UpdateAsync(existingCompany);

            Logo existingLogo = await _cardapioUnitOfWork.LogoRepo.GetByCompanyIDAsync(empresaID);
            Banner existingBanner = await _cardapioUnitOfWork.BannerRepo.GetByCompanyIDAsync(empresaID);

            if (empresaUpdateDTO.ColLogoImagem != null && empresaUpdateDTO.ColLogoImagem.Any(x => x != null))
            {
                var novoLogo = empresaUpdateDTO.ColLogoImagem.FirstOrDefault(x => x != null);
                if (novoLogo != null)
                {
                    if (string.IsNullOrEmpty(empresaUpdateDTO.imageLogoUrlLoadedApi) || (empresaUpdateDTO.imageLogoUrlLoadedApi == "logo_default.png" && (empresaUpdateDTO.ColBannerImagem?.FirstOrDefault()?.Arquivo ?? "") != ""))
                    {
                        if (novoLogo.Arquivo != "logo_default.png")
                        {
                            await UpdateLogo(empresaID, novoLogo, userID);
                        }
                        else
                        {
                            await UpdateLogo(empresaID, new ImageFileAddDTO { Arquivo = "logo_default.png", Nome = "logo_default" }, userID);
                        }
                    }
                    else
                    {
                        if (novoLogo.Arquivo != "logo_default.png")
                        {
                            await UpdateLogo(empresaID, novoLogo, userID);
                        }
                    }
                }
            }

            if (empresaUpdateDTO.ColBannerImagem != null && empresaUpdateDTO.ColBannerImagem.Any(x => x != null && (x.Arquivo ?? "") != ""))
            {
                var novoBanner = empresaUpdateDTO.ColBannerImagem.FirstOrDefault(x => x != null && (x.Arquivo ?? "") != "");
                if (novoBanner != null)
                {
                    if (string.IsNullOrEmpty(empresaUpdateDTO.imageBannerUrlLoadedApi) || empresaUpdateDTO.imageBannerUrlLoadedApi == "banner_default.png")
                    {
                        if (novoBanner.Arquivo != "banner_default.png")
                        {
                            await UpdateBanner(empresaID, novoBanner, userID);
                        }
                        else
                        {
                            await UpdateBanner(empresaID, new ImageFileAddDTO { Arquivo = "banner_default.png", Nome = "banner_default" }, userID);
                        }
                    }
                    else
                    {
                        if (novoBanner.Arquivo != "banner_default.png")
                        {
                            await UpdateBanner(empresaID, novoBanner, userID);
                        }
                    }
                }
            }

            if (!empresaUpdateDTO.Ativo)
            {
                IEnumerable<Infra.Model.User> nonAdminUsers = await _cardapioUnitOfWork.UsuarioRepo.GetAllUserByCompanyIDNonAdmin(empresaID);
                foreach (var usuario in nonAdminUsers)
                {
                    usuario.Ativo = false;
                    await _cardapioUnitOfWork.UsuarioRepo.UpdateAsync(usuario);
                }
            }

            await _cardapioUnitOfWork.Commit();

            if (oldPathBanner != null && oldPathBanner != "banner_default.png")
            {
                string filePathBanner = Path.Combine(basePath, "imagens", "Banner", oldPathBanner);
                FileInfo fileBanner = new FileInfo(filePathBanner);

                if (fileBanner.Exists)
                {
                    fileBanner.Delete();
                }
            }

            if (oldPathLogo != null && oldPathLogo != "logo_default.png")
            {
                string filePathLogo = Path.Combine(basePath, "imagens", "Logo", oldPathLogo);
                FileInfo fileLogo = new FileInfo(filePathLogo);

                if (fileLogo.Exists)
                {
                    fileLogo.Delete();
                }
            }

            return new OkResult();
        }

        public async Task<ActionResult> EditDayDemo(ClaimsPrincipal user, int empresaID, DateDTO dateDTO)
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int userID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(userID);

            Company existingCompany = await _cardapioUnitOfWork.EmpresaRepo.GetByIDAsync(empresaID);
            _validator.VerifyCategoryIdIsPresent(existingCompany.ID);

            existingCompany.DiasDemo = dateDTO.DiasDemo;

            await _cardapioUnitOfWork.EmpresaRepo.UpdateAsync(existingCompany);

            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> VerifyCompanyAlreadyExists(ClaimsPrincipal user, Client.Dto.VerifyCompanyDTO verifyEmpresaDTO)
        {
            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int userID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(userID);

            await Task.Run(() => {
                IQueryable<Company> queryEmpresas = _appDbContext.Empresa.Where(c => c.Nome == verifyEmpresaDTO.EmpresaNome && c.Ativo && !c.Excluido);
                if (verifyEmpresaDTO.ID != null)
                {
                    queryEmpresas = queryEmpresas.Where(c => c.ID != verifyEmpresaDTO.ID);
                }

                foreach (Company empresa in queryEmpresas)
                {
                    _validator.VerifyEmpresaAlreadyExistsByModelData(empresa);
                }

                IQueryable<Company> queryRazaoSocial = _appDbContext.Empresa.Where(c => c.RazaoSocial == verifyEmpresaDTO.RazaoSocial && c.Ativo && !c.Excluido);
                if (verifyEmpresaDTO.ID != null)
                {
                    queryRazaoSocial = queryRazaoSocial.Where(c => c.ID != verifyEmpresaDTO.ID && c.Ativo && !c.Excluido);
                }

                foreach (Company empresa in queryRazaoSocial)
                {
                    _validator.VerifyEmpresaAlreadyExistsByRazaoSocial(empresa);
                }

                if (!string.IsNullOrEmpty(verifyEmpresaDTO.CNPJ))
                {
                    IQueryable<Company> queryCNPJ = _appDbContext.Empresa.Where(c => c.CNPJ == verifyEmpresaDTO.CNPJ && c.Ativo && !c.Excluido);
                    if (verifyEmpresaDTO.ID != null)
                    {
                        queryCNPJ = queryCNPJ.Where(c => c.ID != verifyEmpresaDTO.ID);
                    }

                    foreach (Company empresa in queryCNPJ)
                    {
                        _validator.VerifyEmpresaAlreadyExistsByCNPJ(empresa);
                    }
                }
            });
            return new OkResult();
        }

        private async Task UpdateLogo(int empresaID, ImageFileAddDTO logoDTO, int userID)
        {
            Logo logo = await _cardapioUnitOfWork.LogoRepo.GetByCompanyIDAsync(empresaID);
            Logo newLogo = await CreateImageLogoAsync(logoDTO, userID, empresaID);

            logo.Arquivo = newLogo.Arquivo;
            logo.Nome = newLogo.Nome;
            logo.Altura = newLogo.Altura;
            logo.Largura = newLogo.Largura;
            logo.Ativo = newLogo.Ativo;
            logo.DataCadastro = newLogo.DataCadastro;
            logo.UsuarioIDEdicao = newLogo.UsuarioIDEdicao;
            logo.Tamanho = newLogo.Tamanho;

            await _cardapioUnitOfWork.LogoRepo.UpdateAsync(logo);
        }

        private async Task UpdateBanner(int empresaID, ImageFileAddDTO bannerDTO, int userID)
        {
            Banner banner = await _cardapioUnitOfWork.BannerRepo.GetByCompanyIDAsync(empresaID);
            Banner newBanner = await CreateImageBannerAsync(bannerDTO, userID, empresaID);

            banner.Arquivo = newBanner.Arquivo;
            banner.Nome = newBanner.Nome;
            banner.Altura = newBanner.Altura;
            banner.Largura = newBanner.Largura;
            banner.Ativo = newBanner.Ativo;
            banner.DataCadastro = newBanner.DataCadastro;
            banner.UsuarioIDEdicao = newBanner.UsuarioIDEdicao;
            banner.Tamanho = newBanner.Tamanho;

            await _cardapioUnitOfWork.BannerRepo.UpdateAsync(banner);
        }

        public async Task<ActionResult> DeleteCompany(ClaimsPrincipal User, int empresaID)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr))
                return new BadRequestObjectResult("ID do usuário não encontrado.");
            int UserID = int.Parse(userIdStr);
            _validator.VerifyCategoryIdIsPresent(UserID);

            Company existingCompany = await _cardapioUnitOfWork.EmpresaRepo.GetByIDAsync(empresaID);
            _validator.VerifyCategoryIdIsPresent(existingCompany.ID);

            existingCompany.Ativo = false;
            existingCompany.UsuarioIDEdicao = UserID;
            existingCompany.Excluido = true;

            await _cardapioUnitOfWork.EmpresaRepo.UpdateAsync(existingCompany);
            await _cardapioUnitOfWork.Commit();

            if (existingCompany.ColBanner.First().Arquivo != "banner_default.png")
            {
                string filePathBanner = Path.Combine(basePath, "imagens", "Banner", existingCompany.ColBanner.First().Arquivo);
                FileInfo fileBanner = new FileInfo(filePathBanner);

                if (fileBanner.Exists)
                {
                    fileBanner.Delete();
                }
            }

            if (existingCompany.ColLogo.First().Arquivo != "logo_default.png")
            {
                string filePathLogo = Path.Combine(basePath, "imagens", "Logo", existingCompany.ColLogo.First().Arquivo);
                FileInfo fileLogo = new FileInfo(filePathLogo);

                if (fileLogo.Exists)
                {
                    fileLogo.Delete();
                }
            }

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

        private async Task<GrupoGetDTO> GetGroupAsyncById(int groupID)
        {
            GrupoGetDTO group = await _cardapioUnitOfWork.GrupoRepo.GetGrupoByIDAsync(groupID);

            return group;
        }

        private async Task<UserGroup> GetGroupUserIDAsync(int usuarioID)
        {
            UserGroup groupUser = await _cardapioUnitOfWork.GrupoUsuarioRepo.GetGroupByUserID(usuarioID);

            return groupUser;
        }

        private async Task<IEnumerable<EmpresaGetDTO>> GetEnterprisesIDAsync(int usuarioID, int grupoID, int usuarioTipoID, int empresaID)
        {
            IEnumerable<EmpresaGetDTO> group = await _cardapioUnitOfWork.EmpresaRepo.GetEnterprisesByUsuarioIDAsync(usuarioID, grupoID, usuarioTipoID, empresaID);

            return group;
        }

        private async Task<Logo> CreateImageLogoAsync(ImageFileAddDTO? imagemDTO, int usuarioID, int empresaID)
        {
            string filePath = Path.Combine(basePath, "imagens", "Logo", imagemDTO?.Arquivo ?? string.Empty);
            FileInfo file = new FileInfo(filePath);

            if (!file.Exists)
            {
                throw new FileNotFoundException("Arquivo não encontrado", filePath);
            }

            using (var image = await ImageSharp.LoadAsync(filePath))
            {
                return new Logo
                {
                    Arquivo = imagemDTO?.Arquivo ?? string.Empty,
                    Nome = imagemDTO?.Nome ?? string.Empty,
                    Altura = image.Height,
                    Largura = image.Width,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow,
                    UsuarioIDCadastro = usuarioID,
                    UsuarioIDEdicao = usuarioID,
                    Tamanho = (int)file.Length,
                    EmpresaID = empresaID
                };
            }
        }

        private async Task<Banner> CreateImageBannerAsync(ImageFileAddDTO? imagemDTO, int usuarioID, int empresaID)
        {
            string filePath = Path.Combine(basePath, "imagens", "Banner", imagemDTO?.Arquivo ?? string.Empty);
            FileInfo file = new FileInfo(filePath);

            if (!file.Exists)
            {
                throw new FileNotFoundException("Arquivo não encontrado", filePath);
            }

            using (var image = await ImageSharp.LoadAsync(filePath))
            {
                return new Banner
                {
                    Arquivo = imagemDTO?.Arquivo ?? string.Empty,
                    Nome = imagemDTO?.Nome ?? string.Empty,
                    Altura = image.Height,
                    Largura = image.Width,
                    Ativo = true,
                    DataCadastro = DateTime.UtcNow,
                    DataEdicao = DateTime.UtcNow,
                    UsuarioIDCadastro = usuarioID,
                    UsuarioIDEdicao = usuarioID,
                    Tamanho = (int)file.Length,
                    EmpresaID = empresaID
                };
            }
        }

        private async Task AddImageLogoAsync(Logo imageModel)
        {
            await _cardapioUnitOfWork.LogoRepo.AddAsync(imageModel);
        }

        private async Task AddImageBannerAsync(Banner imageModel)
        {
            await _cardapioUnitOfWork.BannerRepo.AddAsync(imageModel);
        }
        private async Task ProcessAndAddImagesAsync(Company companyModel, IEnumerable<ImageFileAddDTO> LogoDTO, IEnumerable<ImageFileAddDTO> BannerDTO, int usuarioID)
        {
            // <-- troque .First() por .FirstOrDefault()
            if (!LogoDTO.Any() || LogoDTO.FirstOrDefault()?.Arquivo == "")
            {
                var logoModel = new Logo
                {
                    Nome = "logo_default",
                    Arquivo = "logo_default.png",
                    Altura = 80,
                    Largura = 80,
                    Ativo = true,
                    EmpresaID = companyModel.ID,
                    Tamanho = 0,
                    UsuarioIDCadastro = usuarioID,
                    UsuarioIDEdicao = usuarioID,
                    DataCadastro = DateTime.UtcNow,
                    DataEdicao = DateTime.UtcNow,
                };
                await _cardapioUnitOfWork.LogoRepo.AddAsync(logoModel);
                await _cardapioUnitOfWork.Commit();
            }
            else
            {
                foreach (var imagemDTO in LogoDTO)
                {
                    Logo thumbnailProduct = await CreateImageLogoAsync(imagemDTO, usuarioID, companyModel.ID);
                    await AddImageLogoAsync(thumbnailProduct);
                    await _cardapioUnitOfWork.Commit();
                }
            }

            // <-- troque .First() por .FirstOrDefault()
            if (!BannerDTO.Any() || BannerDTO.FirstOrDefault()?.Arquivo == "")
            {
                var bannerModel = new Banner
                {
                    Nome = "banner_default",
                    Arquivo = "banner_default.png",
                    Altura = 1300,
                    Largura = 600,
                    Ativo = true,
                    EmpresaID = companyModel.ID,
                    Tamanho = 0,
                    UsuarioIDCadastro = usuarioID,
                    UsuarioIDEdicao = usuarioID,
                    DataCadastro = DateTime.UtcNow,
                    DataEdicao = DateTime.UtcNow,
                };
                await _cardapioUnitOfWork.BannerRepo.AddAsync(bannerModel);
                await _cardapioUnitOfWork.Commit();
            }
            else
            {
                foreach (var imagemDTO in BannerDTO)
                {
                    Banner imageProduct = await CreateImageBannerAsync(imagemDTO, usuarioID, companyModel.ID);
                    await AddImageBannerAsync(imageProduct);
                    await _cardapioUnitOfWork.Commit();
                }
            }
        }
        private async Task SaveResizedImage(IFormFile file, string path, SizeSharp size)
        {
            using (var image = await ImageSharp.LoadAsync(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = size
                }));

                await image.SaveAsync(path);
            }
        }
    }
}
