using Cardápio.Domain; 
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Microsoft.AspNetCore.Mvc;

using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;


namespace Cardápio.Domain.Codigo
{
    public class CodigoVerificacaoService
    {
        private readonly HttpClient _httpClient;
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly IConfiguration _configuration;
        private readonly DisparosChatService _disparosChat;

        public CodigoVerificacaoService(CardapioUnitOfWork cardapioUnitOfWork, HttpClient httpClient, IConfiguration configuration, DisparosChatService disparosChat)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _httpClient = httpClient;
            _configuration = configuration;
            _disparosChat = disparosChat;
        }

        private string GerarCodigo()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<ActionResult> SendCode(CodigoVerificacaoToAddDTO request)
        {
            if (!string.IsNullOrEmpty(request.Email))
            {
                CodigoVerificacao oldCode = await _cardapioUnitOfWork.CodigoVerificacoRepo.GetByEmail(request.Email);
                if (oldCode != null)
                {
                    oldCode.Ativo = false;
                    await _cardapioUnitOfWork.CodigoVerificacoRepo.UpdateAsync(oldCode);
                    await _cardapioUnitOfWork.Commit();
                }
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                CodigoVerificacao oldCode = await _cardapioUnitOfWork.CodigoVerificacoRepo.GetByPhone(request.Phone);
                if (oldCode != null)
                {
                    oldCode.Ativo = false;
                    await _cardapioUnitOfWork.CodigoVerificacoRepo.UpdateAsync(oldCode);
                }
            }

            string codigo = GerarCodigo();

            CodigoVerificacao codigoVerificacao = new()
            {
                Codigo = codigo,
                Email = request.Email,
                Celular = request.Phone,
                Ativo = true,
                DataCadastro = DateTime.Now,
                DataExpiracao = DateTime.Now.AddMinutes(3),
                DataEdicao = DateTime.Now,
            };

            await _cardapioUnitOfWork.CodigoVerificacoRepo.AddAsync(codigoVerificacao);
            await _cardapioUnitOfWork.Commit();

            if (!string.IsNullOrEmpty(request.Email))
            {
                await SendCodeEmail(request.Email, codigo);
                return new OkObjectResult(new { Message = "Código enviado por e-mail." });
            }

            if (!string.IsNullOrEmpty(request.Phone))
            {
                await SendCodePhone(request.Phone, codigo);
                return new OkObjectResult(new { Message = "Código enviado por telefone." });
            }

            throw new ErrorResponse("Nenhum destino fornecido.", 400);
        }

        public async Task SendCodePhone(string numero, string codigo)
        {
            string message = $"Seu código de verificação é: {codigo}";
            await _disparosChat.ChamarApiChatProAsync(numero, message);
        }

        public async Task SendCodeEmail(string emailTo, string codigo)
        {
            string subject = "Confirme seu e-mail";
            string emailFrom = "suporte@escolha.ai";
            string emailTemplate = $"Seu código de verificação é: {codigo}";
            List<string> emailBccList = null;

            await Task.Run(() => CEmail.SendEmail(subject, emailTemplate, emailFrom, emailTo, emailBccList));
        }

        public async Task<ActionResult> VerifyCode(CodigoVerificacaoValueAddDTO code)
        {
            if (string.IsNullOrEmpty(code.Email) && string.IsNullOrEmpty(code.Phone))
            {
                throw new ErrorResponse("Email ou telefone deve ser fornecido.", 400);
            }

            CodigoVerificacao sendedCode = await GetVerificationCodeAsync(code);
            if (sendedCode == null)
            {
                throw new ErrorResponse("Código inválido.", 400);
            }

            if (sendedCode.DataExpiracao < DateTime.Now)
            {
                await DeactivateCodeAsync(sendedCode);
                throw new ErrorResponse("Código expirado.", 400);
            }

            UsuarioCliente usuarioDB = await GetUserAsync(code);
            UsuarioCliente usuarioCliente = usuarioDB ?? await CreateUserAsync(sendedCode);

            sendedCode.Ativo = false;
            await _cardapioUnitOfWork.Commit();

            AuthenticationDto response = GenerateAuthenticationResponse(usuarioCliente ?? usuarioDB);
            return new OkObjectResult(response);
        }

        private async Task<CodigoVerificacao> GetVerificationCodeAsync(CodigoVerificacaoValueAddDTO code)
        {
            if (!string.IsNullOrEmpty(code.Email))
            {
                return await _cardapioUnitOfWork.CodigoVerificacoRepo.GetByCodeAndEmail(code.Email, code.Code);
            }
            return await _cardapioUnitOfWork.CodigoVerificacoRepo.GetByCodeAndPhone(code.Phone, code.Code);
        }

        private async Task DeactivateCodeAsync(CodigoVerificacao code)
        {
            code.Ativo = false;
            await _cardapioUnitOfWork.Commit();
        }

        private async Task<UsuarioCliente> GetUserAsync(CodigoVerificacaoValueAddDTO code)
        {
            if (!string.IsNullOrEmpty(code.Email))
            {
                return await _cardapioUnitOfWork.UsuarioClienteRepo.GetByEmail(code.Email);
            }
            return await _cardapioUnitOfWork.UsuarioClienteRepo.GetByPhone(code.Phone);
        }

        private async Task<UsuarioCliente> CreateUserAsync(CodigoVerificacao code)
        {
            UsuarioCliente usuarioCliente = new UsuarioCliente
            {
                Email = code.Email,
                Celular = code.Celular,
                Excluido = false,
                DataCadastro = DateTime.UtcNow,
                DataEdicao = DateTime.UtcNow,
                UsuarioTipoID = 5,
                UsuarioIDCadastro = 7,
                UsuarioIDEdicao = 7,
                Ativo = true,
            };

            await _cardapioUnitOfWork.UsuarioClienteRepo.AddAsync(usuarioCliente);
            await _cardapioUnitOfWork.Commit();

            usuarioCliente.UsuarioIDEdicao = usuarioCliente.ID;
            usuarioCliente.UsuarioIDCadastro = usuarioCliente.ID;

            await _cardapioUnitOfWork.UsuarioClienteRepo.UpdateAsync(usuarioCliente);
            await _cardapioUnitOfWork.Commit();

            return usuarioCliente;
        }

        private AuthenticationDto GenerateAuthenticationResponse(UsuarioCliente usuario)
        {
            string generatedToken = GenerateToken(usuario);
            UserAuthenticatedDto userAuthenticatedDto = GenerateUserAuthenticationDto(usuario);
            return GenerateResponseAuthenticatedDto(userAuthenticatedDto, generatedToken);
        }

        private string GenerateToken(UsuarioCliente user)
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
            return tokenHandler.WriteToken(token);
        }

        private UserAuthenticatedDto GenerateUserAuthenticationDto(UsuarioCliente user)
        {
            return new UserAuthenticatedDto(user.Email);
        }

        private AuthenticationDto GenerateResponseAuthenticatedDto(UserAuthenticatedDto user, string token)
        {
            return new AuthenticationDto(user, token);
        }
    }
}