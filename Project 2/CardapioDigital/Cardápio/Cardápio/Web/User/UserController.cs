using Cardápio.Domain.Auth;
using Cardápio.Domain.User;
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Web.User
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpGet("getUsers")]
        public async Task<IActionResult> GetUsers()
        {
            return await HandleServiceResponse(() => _userService.GetUsers(User));
        }

        [Authorize]
        [HttpDelete("deleteUser/{usuarioIDDelete}")]
        public async Task<IActionResult> DeleteUser(int usuarioIDDelete)
        {
            return await HandleServiceResponse(() => _userService.ExcluirUsuario(User, usuarioIDDelete));
        }

        [HttpPost("enviar-codigo")]
        public async Task<IActionResult> EnviarCodigo([FromBody] EnviarCodigoDTO request)
        {
            return await HandleServiceResponse(() => _userService.EnviarCodigoRecuperacao(request.Email));
        }

        [Authorize]
        [HttpPut("editUser/{usuarioIDEdit}")]
        public async Task<IActionResult> EditUser(UsuarioUpdateDTO usuarioEditModel, int usuarioIDEdit)
        {
            return await HandleServiceResponse(() => _userService.EditarUsuario(User, usuarioEditModel, usuarioIDEdit));
        }

        [Authorize]
        [HttpPost("registrar")]
        public async Task<IActionResult> CriarUsuario(UsuarioAddDTO novoUsuario)
        {
            return await HandleServiceResponse(() => _userService.CriarUsuario(User, novoUsuario));
        }

        [Authorize]
        [HttpPost("registrarInCompanyID/{groupID}")]
        public async Task<IActionResult> CriarUsuarioInCompanyID(UsuarioAddDTO novoUsuario, int groupID)
        {
            return await HandleServiceResponse(() => _userService.CriarUsuarioInCompanyID(User, novoUsuario, groupID));
        }

        [Authorize]
        [HttpPost("complete-signup")]
        public async Task<IActionResult> CompletarSignup(CompletarCadastroAddDTO completarCadastroAddDTO)
        {
            return await HandleServiceResponse(() => _userService.CompletarSignup(User, completarCadastroAddDTO));
        }

        [HttpPost("trocarSenha")]
        public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaDTO dto)
        {
            return await _userService.TrocarSenha(dto.Email, dto.NovaSenha, dto.CodigoVerificacao);

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
