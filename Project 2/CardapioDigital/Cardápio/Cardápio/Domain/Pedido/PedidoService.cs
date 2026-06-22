using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cardápio.Domain.Pedido
{
    public class PedidoService
    {
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;

        public PedidoService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext, Validator validator)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
        }

        public async Task<ActionResult> GetAllPedidos(ClaimsPrincipal User, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            IEnumerable<TodosPedidoGetDTO> pedidos = await _cardapioUnitOfWork.PedidoRepo.GetDTOByEmpresaID(empresaID);

            return new OkObjectResult(pedidos);
        }

        public async Task<ActionResult> CreateNewPedido(ClaimsPrincipal User, int empresaID, PedidoAddDTO pedidoAddDTO)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);



            return new OkObjectResult(new());
        }
    }
}
