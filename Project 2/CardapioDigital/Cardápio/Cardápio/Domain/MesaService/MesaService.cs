using Cardápio.Dto;
using Cardápio.Infra.Data;
using Cardápio.Infra.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cardápio.Infra.Model;
using Cardápio.Domain.QrCode;

namespace Cardápio.Domain.MesaService
{
    public class MesaService
    {
        private readonly CardapioUnitOfWork _cardapioUnitOfWork;
        private readonly AppDbContext _appDbContext;
        private readonly Validator _validator;
        private readonly QrCodeService _qrCodeService;
        string basePath = AppDomain.CurrentDomain.BaseDirectory;

        public MesaService(CardapioUnitOfWork cardapioUnitOfWork, AppDbContext appDbContext, Validator validator, QrCodeService qrCodeService)
        {
            _cardapioUnitOfWork = cardapioUnitOfWork;
            _appDbContext = appDbContext;
            _validator = validator;
            _qrCodeService = qrCodeService;
        }

        public async Task<ActionResult> GetMesas(ClaimsPrincipal User, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            IEnumerable<MesaGetDTO> mesas = await _cardapioUnitOfWork.MesaRepo.GetAllMesas(empresaID);

            return new OkObjectResult(mesas);
        }

        public async Task<ActionResult> CreateNewMesa(ClaimsPrincipal User, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            int mesasCount = await _cardapioUnitOfWork.MesaRepo.CountMesasAtivas(empresaID);

            // Buscar o nome da empresa selecionada, não da empresa principal do usuário
            var empresa = await _cardapioUnitOfWork.EmpresaRepo.GetByIDAsync(empresaID);
            string empresaNome = empresa?.Nome ?? "";

            Mesa nextMesa = new()
            {
                Ativo = true,
                NomeMesa = "Mesa " + (mesasCount + 1),
                EmpresaID = empresaID,
                MesaStatusID = 1,
                UsuarioIDCadastro = UserID,
                UsuarioIDEdicao = UserID,
                DataCadastro = new DateTime(),
                DataEdicao = new DateTime(),
            };

            nextMesa.QRCode = await _qrCodeService.SaveQrCode(empresaNome, nextMesa.NomeMesa);

            await _cardapioUnitOfWork.MesaRepo.AddAsync(nextMesa);
            await _cardapioUnitOfWork.Commit();

            return new OkObjectResult(nextMesa);
        }

        private void DeleteOldQrCode(string qrCodePath)
        {
            string filePath = Path.Combine(basePath, "imagens", "QrCode", qrCodePath);

            if (!string.IsNullOrEmpty(qrCodePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public async Task<ActionResult> EditMesa(ClaimsPrincipal User, int empresaID, MesaUpdateDTO mesa)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            await VerifyMesaAlreadyExists(UserID ?? 0, mesa, empresaID);

            // Buscar o nome da empresa selecionada, não da empresa principal do usuário
            var empresa = await _cardapioUnitOfWork.EmpresaRepo.GetByIDAsync(empresaID);
            string empresaNome = empresa?.Nome ?? "";

            Mesa mesaDB = await _cardapioUnitOfWork.MesaRepo.GetByIDAsync(mesa.ID, empresaID);

            DeleteOldQrCode(mesaDB.QRCode);

            mesaDB.NomeMesa = mesa.NomeMesa;
            mesaDB.QRCode = await _qrCodeService.SaveQrCode(empresaNome, mesaDB.NomeMesa);

            await _cardapioUnitOfWork.MesaRepo.UpdateAsync(mesaDB);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task<ActionResult> DeleteMesa(ClaimsPrincipal User, int mesaID, int empresaID)
        {
            int? UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _validator.VerifyUserIDIsPresent(UserID);

            Mesa mesaDB = await _cardapioUnitOfWork.MesaRepo.GetByIDAsync(mesaID, empresaID);

            DeleteOldQrCode(mesaDB.QRCode);

            mesaDB.Ativo = false;

            await _cardapioUnitOfWork.MesaRepo.UpdateAsync(mesaDB);
            await _cardapioUnitOfWork.Commit();

            return new OkResult();
        }

        public async Task VerifyMesaAlreadyExists(int UserID, MesaUpdateDTO verifyMesaDTO, int empresaID)
        {
            IEnumerable<Mesa> queryMesa = _appDbContext.Mesa.Where(c => c.NomeMesa == verifyMesaDTO.NomeMesa
                                                               && c.Ativo
                                                               && c.EmpresaID == empresaID).ToList();

            if (verifyMesaDTO.ID != null)
            {
                queryMesa = queryMesa.Where(c => c.ID != verifyMesaDTO.ID);
            }

            foreach (Mesa mesa in queryMesa)
            {
                _validator.VerifyMesaAlreadyExists(mesa);
            }
        }
    }
}
