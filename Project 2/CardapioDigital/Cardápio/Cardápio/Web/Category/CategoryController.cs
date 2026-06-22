using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cardápio.Dto;
using Cardápio.Infra.Helpers;
using Cardápio.Domain.Category;
using Cardápio.Infra.Data;

namespace Cardápio.Web.Category
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("GetCategoriesByCompanyID/{companyID}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CategoriaGetDTO>>> GetDataByCompanyID(int companyID)
        {
            return await HandleServiceResponse(() => _categoryService.GetDataByCompanyID(User, companyID));
        }

        [HttpGet("GetCategoriesByCompanyName/{companyName}")]
        public async Task<ActionResult<IEnumerable<CategoriaGetDTO>>> GetDataByCompanyName(string companyName)
        {
            return await HandleServiceResponse(() => _categoryService.GetDataByCompanyName(companyName));
        }

        [HttpPut("UpdateCategoriaOrdem")]
        [Authorize]
        public async Task<ActionResult> UpdateCategoriaOrdemAsync(ICollection<CategoriaUpdateDTO> categoriasAtualizadas)
        {
            return await HandleServiceResponse(() => _categoryService.UpdateCategoriaOrdemAsync(User, categoriasAtualizadas));
        }

        [HttpPost("CreateCategory/{empresaID}")]
        [Authorize]
        public async Task<ActionResult> AddCategoriaAsync(CategoriaAddDTO categoriaAddDTO, int empresaID)
        {
            return await HandleServiceResponse(() => _categoryService.AddCategoriaAsync(User, categoriaAddDTO, empresaID));
        }

        [HttpPut("updateCategory/{categoriaID}")]
        [Authorize]
        public async Task<ActionResult> UpdateCategoriaAsync(CategoriaUpdateDTO categoriaUpdateDTO, int categoriaID)
        {
            return await HandleServiceResponse(() => _categoryService.UpdateCategoriaAsync(User, categoriaID, categoriaUpdateDTO));
        }

        [HttpDelete("deleteCategory/{categoriaID}/{empresaID}")]
        [Authorize]
        public async Task<ActionResult> DeleteCategoriaAsync(int categoriaID, int empresaID)
        {
            return await HandleServiceResponse(() => _categoryService.DeleteCategoriaAsync(User, categoriaID, empresaID));
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
