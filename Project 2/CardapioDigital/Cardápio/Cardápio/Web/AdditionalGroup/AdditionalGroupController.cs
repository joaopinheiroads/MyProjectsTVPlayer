using Cardápio.Domain.AdditionalGroup;
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Web.AdditionalGroup
{
    [ApiController]
    [Route("api/additional-group")]
    public class AdditionalGroupController : ControllerBase
    {
        private readonly AdditionalGroupService _additionalGroupService;

        public AdditionalGroupController(AdditionalGroupService additionalGroupService)
        {
            _additionalGroupService = additionalGroupService;
        }

        [HttpGet("data/{companyID}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GrupoAdicionalGetDTO>>> GetDataAdditionalGroup(int companyID)
        {
            return await HandleServiceResponse(() => _additionalGroupService.GetDataAdditionalGroup(User, companyID)); 
        }

        [HttpGet("data/{productID}/product")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GrupoAdicionalGetDTO>>> GetDataAdditionalGroupByProductID(int productID)
        {
            return await HandleServiceResponse(() => _additionalGroupService.GetDataAdditionalGroupByProductID(User, productID));
        }

        [HttpGet("data/{companyID}/with-product")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GrupoAdicionalGetDTO>>> GetDataAdditionalGroupWithProduct(int companyID)
        {
            return await HandleServiceResponse(() => _additionalGroupService.GetDataAdditionalGroupWithProduct(User, companyID));
        }

        [HttpGet("verify-products/{groupID}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ProdutoGetDTO>>> VerifyProducts(int groupID)
        {
            return await HandleServiceResponse(() => _additionalGroupService.VerifyProducts(User, groupID));
        }

        [HttpPost("data")]
        [Authorize]
        public async Task<ActionResult<int>> SaveNewGroupAdditional(GrupoAdicionalAddDTO grupoAdicionalAdd)
        {
            return await HandleServiceResponse(() => _additionalGroupService.SaveNewGroupAdditional(User, grupoAdicionalAdd));
        }

        [HttpPut("data")]
        [Authorize]
        public async Task<ActionResult> EditGroupAdditional(GrupoAdicionalUpdateDTO grupoAdicionalAdd)
        {
            return await HandleServiceResponse(() => _additionalGroupService.EditGroupAdditional(User, grupoAdicionalAdd));
        }

        [HttpPut("data/unlink/{grupoID}/{produtoID}")]
        [Authorize]
        public async Task<ActionResult> UnlinkGroupFromProduct(int grupoID, int produtoID)
        {
            return await HandleServiceResponse(() => _additionalGroupService.UnlinkGroupFromProduct(User, grupoID, produtoID));
        }

        [HttpDelete("data/{productID}/{empresaID}")]
        [Authorize]
        public async Task<ActionResult> DeleteGroupAdditional(int productID, int empresaID)
        {
            return await HandleServiceResponse(() => _additionalGroupService.DeleteGroupAdditional(User, productID, empresaID));
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
