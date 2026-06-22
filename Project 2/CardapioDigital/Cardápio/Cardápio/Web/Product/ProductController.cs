using Cardápio.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cardápio.Infra.Helpers;
using Cardápio.Domain.Product;

namespace Cardápio.Web.Product
{
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [Authorize]
        [HttpGet("GetProductsByCompanyID/{empresaID}")]
        public async Task<ActionResult<IEnumerable<ProdutoGetDTO>>> GetProdutoByEmpresaIDAsync(int empresaID)
        {
            return await HandleServiceResponse(() => _productService.GetProdutoByEmpresaIDAsync(User, empresaID));
        }

        [HttpGet("GetProductsByCompanyName/{empresaNome}")]
        public async Task<ActionResult<IEnumerable<ProdutoGetDTO>>> GetProdutoByEmpresaNomeAsync(string empresaNome)
        {
            return await HandleServiceResponse(() => _productService.GetProdutoByEmpresaNomeAsync(empresaNome));
        }

        // 🔓 ROTA PÚBLICA para cardápio - SEM autenticação necessária
        [HttpGet("GetProductsByCompanyNamePublic/{empresaNome}")]
        public async Task<ActionResult<IEnumerable<ProdutoGetDTO>>> GetProdutoByEmpresaNomePublicAsync(string empresaNome)
        {
            return await HandleServiceResponse(() => _productService.GetProdutoByEmpresaNomeAsync(empresaNome));
        }

        [Authorize]
        [HttpGet("GetProductsByGroupID/{groupID}")]
        public async Task<ActionResult<IEnumerable<ProdutoGetDTO>>> GetProdutoByGroupID(int groupID)
        {
            return await HandleServiceResponse(() => _productService.GetProdutoByGroupID(User, groupID));
        }

        [Authorize]
        [HttpGet("GetProductByID/{productID}")]
        public async Task<ActionResult<ProdutoGetDTO>> GetProdutoByID(int productID)
        {
            return await HandleServiceResponse(() => _productService.GetProdutoByID(User, productID));
        }

        // 🔓 ROTA PÚBLICA para cardápio - SEM autenticação necessária
        [HttpGet("GetProductByIDPublic/{productID}/{empresaNome}")]
        public async Task<ActionResult<ProdutoGetDTO>> GetProdutoByIDPublic(int productID, string empresaNome)
        {
            return await HandleServiceResponse(() => _productService.GetProdutoByIDPublic(productID, empresaNome));
        }

        [Authorize]
        [HttpPost("SaveProduct/{empresaSelectedID}")]
        public async Task<ActionResult> AddProducts(ProdutoAddDTO product, int empresaSelectedID)
        {
            if (product?.PromocoesPorHorario != null)
            {
                foreach (var promocao in product.PromocoesPorHorario)
                {
                    Console.WriteLine($"[ProductController] 📥 Promoção recebida: {promocao.DiaSemana} {promocao.HoraInicio}-{promocao.HoraFim} R$ {promocao.PrecoPromocional:F2}");
                }
            }
            return await HandleServiceResponse(() => _productService.AddProducts(User, product, empresaSelectedID));
        }

        [Authorize]
        [HttpPut("EditProduct/{productID}")]
        public async Task<ActionResult> UpdateProducts(ProdutoAddDTO product, int productID)
        {
            return await HandleServiceResponse(() => _productService.UpdateProducts(User, product, productID));
        }

        [Authorize]
        [HttpDelete("DeleteProduct/{grupoID}/{empresaID}")]
        public async Task<ActionResult> DeactivateProduct(int grupoID, int empresaID)
        {
            return await HandleServiceResponse(() => _productService.DeactivateProduct(User, grupoID, empresaID));
        }

        [Authorize]
        [HttpDelete("DeletePromotion/{promocaoId}")]
        public async Task<ActionResult> DeletePromotion(int promocaoId)
        {
            return await HandleServiceResponse(() => _productService.DeletePromotion(User, promocaoId));
        }

        [Authorize]
        [HttpPost("ExportProducts")]
        public async Task<ActionResult> ExportProduct([FromBody] ExportarProdutosDTO exportDto)
        {
            return await HandleServiceResponse(() => _productService.ExportProduct(User, exportDto));
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
