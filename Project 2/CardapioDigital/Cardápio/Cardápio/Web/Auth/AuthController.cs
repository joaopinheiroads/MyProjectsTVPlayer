using Cardápio.Domain.Auth;
using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Cardápio.Infra.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Web.Auth
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _appDbContext;
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;

        public AuthController(AuthService authService, AppDbContext appDbContext, CardapioUnitOfWork cardapioUnitOfWork)
        {
            _authService = authService;
            _appDbContext = appDbContext;
            _cardapioUnitOfWork = cardapioUnitOfWork;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            return await HandleServiceResponse(() => _authService.LoginOwner(loginModel));
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] NovoUsuarioSistemaDTO userModel)
        {
            return await HandleServiceResponse(() => _authService.SignupOwner(userModel));
        }

        [HttpPost("google-authentication")]
        public async Task<IActionResult> GoogleAuthentication([FromBody] GoogleAutenticationDTO googleAutenticationDTO)
        {
            return await HandleServiceResponse(() => _authService.GoogleAuthentication(googleAutenticationDTO));
        }

        [HttpGet("confirmAccount/{SolicitID}")]
        public async Task<IActionResult> ConfirmAccount([FromRoute] Guid SolicitID)
        {
            return await HandleServiceResponse(() => _authService.ConfirmAccount(SolicitID));
        }

        [Authorize]
        [HttpGet("getCompanyID")]
        public async Task<IActionResult> GetCompanyID()
        {
            return await HandleServiceResponse(() => _authService.GetCompanyID(User));
        }

        [Authorize]
        [HttpGet("client-user")]
        public async Task<IActionResult> GetClientUserCredentials()
        {
            return await HandleServiceResponse(() => _authService.GetClientUserCredentials(User));
        }

        private async Task<ActionResult> HandleServiceResponse(Func<Task<ActionResult>> serviceMethod)
        {
            try
            {
                ActionResult result = await serviceMethod();

                if (result is OkObjectResult okResult)
                {
                    return Ok(okResult.Value);
                }

                return Ok(result);
            }
            catch (ErrorResponse ex)
            {
                ErrorResponseDTO errorResponseDto = new ErrorResponseDTO(ex);

                return new ObjectResult(errorResponseDto)
                {
                    StatusCode = ex.CodeError
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse($"Erro interno: {ex.Message}", 500));
            }
        }
    }
}
