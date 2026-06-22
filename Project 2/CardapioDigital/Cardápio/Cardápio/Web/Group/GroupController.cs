using Cardápio.Domain.Group;
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Web.Group
{
    [ApiController]
    [Route("api/group")]
    public class GroupController : ControllerBase
    {
        private readonly GroupService _groupService;

        public GroupController(GroupService groupService)
        {
            _groupService = groupService;
        }

        [Authorize]
        [HttpGet("GetGroups")]
        public async Task<IActionResult> GetGroups()
        {
            return await HandleServiceResponse(() => _groupService.GetGroups(User));
        }

        [Authorize]
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup(GrupoAddDTO grupoInfos)
        {
            return await HandleServiceResponse(() => _groupService.CreateGroup(User, grupoInfos));

        }

        [Authorize]
        [HttpPut("UpdateGroup/{groupID}")]
        public async Task<IActionResult> UpdateGroup(int groupID, GrupoUpdateDTO grupoInfos)
        {
            return await HandleServiceResponse(() => _groupService.UpdateGroup(User, grupoInfos, groupID));
        }

        [Authorize]
        [HttpPut("ReturnGroup/{groupID}")]
        public async Task<IActionResult> ReturnGroup(int groupID, [FromQuery] int? empresaID = null)
        {
            return await HandleServiceResponse(() => _groupService.ReturnGroup(User, groupID, empresaID));
        }

        [Authorize]
        [HttpDelete("DeleteGroup/{groupID}")]
        public async Task<IActionResult> DeleteGroup(int groupID)
        {
            return await HandleServiceResponse(() => _groupService.DeleteGroup(User, groupID));
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
                var innerMsg = ex.InnerException?.Message ?? "sem inner exception";
                return StatusCode(500, new ErrorResponse($"Erro interno: {ex.Message} | Inner: {innerMsg}", 500));
            }
        }
    }
}
