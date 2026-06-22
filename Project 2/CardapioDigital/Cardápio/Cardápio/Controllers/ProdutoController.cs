using Microsoft.AspNetCore.Mvc;
using Cardápio.Dto;
using Cardápio.Services;

namespace Cardápio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoService _produtoService;

        public ProdutoController(ProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProdutoAddDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Chama o novo m�todo AddAsync
            var id = await _produtoService.AddAsync(dto);
            return CreatedAtAction(nameof(Add), new { id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProdutoUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Normaliza os campos de promo��o antes de atualizar
            dto.NormalizePromotionFields();

            var result = await _produtoService.UpdateAsync(id, dto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var produtos = await _produtoService.GetAllAsync();
            return Ok(produtos);
        }
    }
}