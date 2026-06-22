using Cardápio.Client.Infra.Crypto;
using Cardápio.Client.Pages.Usuarios;
using Cardápio.Domain.QrCode;
using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cardápio.Domain.Auth
{
    public class UnauthorizedAccessException : Exception
    {
        public UnauthorizedAccessException(string message) : base(message) { }
    }

    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly Validator _validator;
        private readonly QrCodeService _qrCodeService;
        private readonly Crypto _crypto;

        public AuthService(AppDbContext context, CardapioUnitOfWork cardapioUnitOfWork,
                            IConfiguration configuration, Validator validator, QrCodeService qrCodeService,
                            Crypto crypto)
        {
            _context = context;
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _configuration = configuration;
            _validator = validator;
            _qrCodeService = qrCodeService;
            _crypto = crypto;
        }

        public async Task<ActionResult> LoginOwner(LoginModel loginModel)
        {
            Infra.Model.User user = GetUser(loginModel).GetAwaiter().GetResult();

            _validator.VerifyCredentialsUser(user, loginModel);
            _validator.VerifyUserIDIsPresent(user.ID);

            string generatedToken = GenerateToken(user);
            UserAuthenticatedDto userAuthenticatedDto = GenerateUserAuthenticationDto(user);
            AuthenticationDto ResponseAuthenticatedDto = GenerateResponseAuthenticatedDto(userAuthenticatedDto, generatedToken);

            return new OkObjectResult(ResponseAuthenticatedDto);
        }

        public async Task<ActionResult> SignupOwner(NovoUsuarioSistemaDTO userModel)
        {
            Infra.Model.User user = GetUserByEmail(userModel.Email).GetAwaiter().GetResult();
            _validator.VerifyUserAlreadyExists(user);

            EmpresaGetDTO company = await _cardapioUnitOfWork.EmpresaRepo.GetEmpresaByNameAsync(userModel.EmpresaNome);
            _validator.VerifyEmpresaAlreadyExists(company);

            Infra.Model.Group grupoModel = new Infra.Model.Group
            {
                Nome = userModel.Nome,
                UsuarioIDCadastro = 7,
                UsuarioIDEdicao = 7,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
                GrupoTipoID = 1,
                Excluido = false,
                Ativo = false,
            };

            await _cardapioUnitOfWork.GrupoRepo.AddAsync(grupoModel);
            await _cardapioUnitOfWork.Commit();

            Company empresaModel = new Company
            {
                Nome = userModel.EmpresaNome,
                RazaoSocial = userModel.EmpresaNome,
                UsuarioIDCadastro = 7,
                UsuarioIDEdicao = 7,
                CEP = userModel.Cep.Replace("-", "").Trim(),
                Celular = userModel.Celular.Trim(),
                Telefone = userModel.Telefone.Trim(),
                EstadoID = userModel.EstadoID.Trim(),
                CidadeID = userModel.CidadeID,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
                Ativo = false,
                Excluido = false,
                DiasDemo = 10,
                EmpresaTipoID = 1,
                GrupoID = grupoModel.ID
            };

            await _cardapioUnitOfWork.EmpresaRepo.AddAsync(empresaModel);
            await _cardapioUnitOfWork.Commit();

            empresaModel.QRCode = await _qrCodeService.SaveQrCode(empresaModel.Nome);

            Infra.Model.User usuarioModel = new Infra.Model.User
            {
                Nome = userModel.Nome,
                Password = userModel.Senha,
                Email = userModel.Email,
                EmpresaID = empresaModel.ID,
                GrupoID = grupoModel.ID,
                Excluido = false,
                UsuarioTipoID = 1,
                UsuarioStatusID = 1,
                UsuarioIDCadastro = 7,
                UsuarioIDEdicao = 7,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
                Ativo = true
            };

            await _cardapioUnitOfWork.UsuarioRepo.AddAsync(usuarioModel);
            await _cardapioUnitOfWork.Commit();

            var usuarioIDReal = usuarioModel.ID;

            grupoModel.UsuarioIDCadastro = usuarioIDReal;
            grupoModel.UsuarioIDEdicao = usuarioIDReal;
            await _cardapioUnitOfWork.GrupoRepo.UpdateAsync(grupoModel);

            empresaModel.UsuarioIDCadastro = usuarioIDReal;
            empresaModel.UsuarioIDEdicao = usuarioIDReal;
            await _cardapioUnitOfWork.EmpresaRepo.UpdateAsync(empresaModel);

            usuarioModel.UsuarioIDCadastro = usuarioIDReal;
            usuarioModel.UsuarioIDEdicao = usuarioIDReal;
            await _cardapioUnitOfWork.UsuarioRepo.UpdateAsync(usuarioModel);

            await _cardapioUnitOfWork.Commit();

            var usuarioGrupoModel = new UserGroup
            {
                GrupoID = grupoModel.ID,
                IsAdmin = true,
                UsuarioID = usuarioIDReal,
                UsuarioIDCadastro = usuarioIDReal,
                UsuarioIDEdicao = usuarioIDReal,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now,
                Ativo = true
            };

            await _cardapioUnitOfWork.GrupoUsuarioRepo.AddAsync(usuarioGrupoModel);
            await _cardapioUnitOfWork.Commit();

            var usuarioEmpresaModel = new UsuarioEmpresa
            {
                EmpresaID = empresaModel.ID,
                UsuarioID = usuarioIDReal
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
                UsuarioIDCadastro = usuarioIDReal,
                UsuarioIDEdicao = usuarioIDReal,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now
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
                UsuarioIDCadastro = usuarioIDReal,
                UsuarioIDEdicao = usuarioIDReal,
                DataCadastro = DateTime.Now,
                DataEdicao = DateTime.Now
            };

            await _cardapioUnitOfWork.BannerRepo.AddAsync(bannerModel);
            await _cardapioUnitOfWork.Commit();

            var SolicitacaoDemonstracaoModel = new SolicitacaoDemonstracao
            {
                GrupoID = grupoModel.ID,
                Ativo = true,
            };

            await _cardapioUnitOfWork.SolicitacaoRepo.AddAsync(SolicitacaoDemonstracaoModel);
            await _cardapioUnitOfWork.Commit();

            string emailTemplate = await File.ReadAllTextAsync(@"./wwwroot/EmailModel.html");
            string linkDinamico = "https://191.6.5.106:44303/confirmacao/" + SolicitacaoDemonstracaoModel.ID;
            emailTemplate = emailTemplate.Replace("link-customizado", linkDinamico);

            _ = Task.Run(() => SendEmailAsync(userModel.Email, emailTemplate));

            return new OkResult();
        }

        public async Task<ActionResult> GoogleAuthentication(GoogleAutenticationDTO googleAutenticationDTO)
        {
            UsuarioCliente usuarioClienteDB = await _cardapioUnitOfWork.UsuarioClienteRepo.GetByEmail(googleAutenticationDTO.Email);

            string generatedToken;
            UserAuthenticatedDto userAuthenticatedDto;
            object response;

            if (usuarioClienteDB != null)
            {
                generatedToken = GenerateTokenToClient(usuarioClienteDB);
                userAuthenticatedDto = GenerateUserAuthenticationDtoToClient(usuarioClienteDB);
                response = GenerateResponseAuthenticatedDto(userAuthenticatedDto, generatedToken);
                return new OkObjectResult(response);
            }

            UsuarioCliente usuarioCliente = new UsuarioCliente
            {
                Email = googleAutenticationDTO.Email,
                DataCadastro = DateTime.UtcNow,
                DataEdicao = DateTime.UtcNow,
                UsuarioTipoID = 5,
                UsuarioIDCadastro = 7,
                UsuarioIDEdicao = 7,
                Ativo = true,
                Excluido = false,
            };

            await _cardapioUnitOfWork.UsuarioClienteRepo.AddAsync(usuarioCliente);
            await _cardapioUnitOfWork.Commit();

            usuarioCliente.UsuarioIDEdicao = usuarioCliente.ID;
            usuarioCliente.UsuarioIDCadastro = usuarioCliente.ID;

            await _cardapioUnitOfWork.UsuarioClienteRepo.UpdateAsync(usuarioCliente);
            await _cardapioUnitOfWork.Commit();

            generatedToken = GenerateTokenToClient(usuarioCliente);
            userAuthenticatedDto = GenerateUserAuthenticationDtoToClient(usuarioCliente);
            response = GenerateResponseAuthenticatedDto(userAuthenticatedDto, generatedToken);

            return new OkObjectResult(response);
        }

        private async Task SendEmailAsync(string emailTo, string emailTemplate)
        {
            string subject = "Confirme seu e-mail";
            string emailFrom = "contato@tvplayer.com.br";
            List<string> emailBccList = null;

            CEmail.SendEmail(subject, emailTemplate, emailFrom, emailTo, emailBccList);
        }

        public async Task<ActionResult> ConfirmAccount(Guid SolicitID)
        {
            if (SolicitID == Guid.Empty)
            {
                throw new ErrorResponse("ID inválido.", 404);
            }

            SolicitacaoDemonstracao solicitacaoModel = await _cardapioUnitOfWork.SolicitacaoRepo.GetBySolicitIDAsync(SolicitID);

            if (solicitacaoModel == null || solicitacaoModel.Ativo == false)
            {
                throw new ErrorResponse("Solicitação não encontrada.", 404);
            }

            Infra.Model.Group groupModel = await _cardapioUnitOfWork.GrupoRepo.GetGrupoModelByIDAsync(solicitacaoModel.GrupoID ?? 0);

            IEnumerable<Company> companyModel = await _cardapioUnitOfWork.EmpresaRepo.GetEnterprisesModelByGrupoID(groupModel.ID);

            groupModel.Ativo = true;
            await _cardapioUnitOfWork.GrupoRepo.UpdateAsync(groupModel);
            await _cardapioUnitOfWork.Commit();

            foreach (var company in companyModel)
            {
                company.Ativo = true;

                await _cardapioUnitOfWork.EmpresaRepo.UpdateAsync(company);
                await _cardapioUnitOfWork.Commit();
            }

            solicitacaoModel.Ativo = false;
            await _cardapioUnitOfWork.SolicitacaoRepo.UpdateAsync(solicitacaoModel);

            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> GetCompanyID(ClaimsPrincipal User)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            UsuarioGetDTO usuarioDB = await GetEmpresaIDAsync(UserID);
            if (usuarioDB == null)
            {
                throw new ErrorResponse("usuario não encontrado", 401);
            }
            _validator.VerifyUserIsPresent(usuarioDB);

            return new OkObjectResult(usuarioDB);
        }

        public async Task<ActionResult> GetClientUserCredentials(ClaimsPrincipal User)
        {
            int UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            UsuarioClienteGetDTO usuarioCliente = await _cardapioUnitOfWork.UsuarioClienteRepo.GetByIDAsyncDTO(UserID);
            return new OkObjectResult(usuarioCliente);
        }

        private async Task<UsuarioGetDTO> GetEmpresaIDAsync(int usuarioID)
        {
            UsuarioGetDTO user = await _cardapioUnitOfWork.UsuarioRepo.GetUsuarioByIDAsync(usuarioID);

            return user;
        }

        private async Task<Infra.Model.User> GetUser(LoginModel loginModel)
        {
            var usuarioDb = await (from usuario in _context.Usuario
                                   join usuarioTipo in _context.UsuarioTipo on usuario.UsuarioTipoID equals usuarioTipo.ID
                                   join usuarioStatus in _context.UsuarioStatus on usuario.UsuarioStatusID equals usuarioStatus.ID
                                   join empresa in _context.Empresa on usuario.EmpresaID equals empresa.ID
                                   where usuario.Email == loginModel.Login &&
                                         usuario.Password == loginModel.Password &&
                                         usuario.Ativo == true &&
                                         usuario.Excluido == false &&
                                         usuarioTipo.Ativo == true &&
                                         usuarioStatus.Ativo == true &&
                                         empresa.Ativo == true
                                   select new
                                   {
                                       usuario.ID,
                                       usuario.Nome,
                                       usuario.Email,
                                       usuario.Password,
                                       empresa.EmpresaTipoID,
                                       empresa.DataCadastro,
                                       empresa.DiasDemo
                                   }).FirstOrDefaultAsync();

            if (usuarioDb == null || usuarioDb.EmpresaTipoID == 1 && usuarioDb.DataCadastro.AddDays(usuarioDb.DiasDemo ?? 0) <= DateTime.Now)
                return null;

            return new Infra.Model.User
            {
                ID = usuarioDb.ID,
                Nome = usuarioDb.Nome,
                Email = usuarioDb.Email,
                Password = usuarioDb.Password
            };
        }

        private async Task<Infra.Model.User> GetUserByEmail(string userEmail)
        {
            var usuarioDb = await (from usuario in _context.Usuario
                                   join empresa in _context.Empresa on usuario.EmpresaID equals empresa.ID
                                   where usuario.Email == userEmail
                                   select new
                                   {
                                       usuario.ID
                                   }).FirstOrDefaultAsync();

            if (usuarioDb == null)
            {
                return null;
            }

            return new Infra.Model.User
            {
                ID = usuarioDb.ID
            };
        }

        private string GenerateTokenToClient(UsuarioCliente usuarioCliente)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, usuarioCliente.ID.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        private string GenerateToken(Infra.Model.User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiresInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            string tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        private UserAuthenticatedDto GenerateUserAuthenticationDtoToClient(UsuarioCliente user)
        {
            return new UserAuthenticatedDto(user.Email);
        }

        private UserAuthenticatedDto GenerateUserAuthenticationDto(Infra.Model.User user)
        {
            return new UserAuthenticatedDto(user.Email);
        }

        private AuthenticationDto GenerateResponseAuthenticatedDto(UserAuthenticatedDto user, string token)
        {
            return new AuthenticationDto(user, token);
        }
    }
}
