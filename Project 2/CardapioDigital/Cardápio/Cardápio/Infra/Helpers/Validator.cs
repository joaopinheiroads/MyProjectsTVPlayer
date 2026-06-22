using Cardápio.Client.Pages.Empresas;
using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Helpers
{
    public class Validator
    {
        private void ValidateId(int? id, string errorMessage, int errorCode)
        {
            if (id == null || id == 0)
            {
                throw new ErrorResponse(errorMessage, errorCode);
            }
        }

        private void ValidateCondition(bool condition, string errorMessage, int errorCode)
        {
            if (condition)
            {
                throw new ErrorResponse(errorMessage, errorCode);
            }
        }

        public void VerifyUserIDIsPresent(int? userID)
        {
            ValidateId(userID, "Usuário não encontrado.", 404);
        }

        public void VerifyUserIsPresent(UsuarioGetDTO usuario)
        {
            ValidateCondition(usuario == null && usuario.ID != 0, "Usuário não encontrado.", 404);
        }

        public void VerifyUserAlreadyExists(User user)
        {
            ValidateCondition(user != null && user.ID != 0, "Email já existe.", 404);
        }

        public void VerifyEmpresaAlreadyExists(EmpresaGetDTO empresa)
        {
            ValidateCondition(empresa != null && empresa.ID != 0, "Empresa já registrado.", 404);
        }

        public void VerifyEmpresaAlreadyExistsByModelData(Company empresa)
        {
            ValidateCondition(empresa != null && empresa.ID != 0, "Empresa já registrado.", 404);
        }

        public void VerifyEmpresaAlreadyExistsByRazaoSocial(Company empresa)
        {
            ValidateCondition(empresa != null && empresa.ID != 0, "Razão social já registrado.", 404);
        }

        public void VerifyEmpresaAlreadyExistsByCNPJ(Company empresa)
        {
            ValidateCondition(empresa != null && empresa.ID != 0, "CNPJ já registrado.", 404);
        }

        public void VerifyCompanyIdIsPresent(int? companyID)
        {
            ValidateId(companyID, "Empresa não encontrada.", 404);
        }

        public void VerifyCategoryIdIsPresent(int? categoryID)
        {
            ValidateId(categoryID, "Categoria não encontrada.", 404);
        }

        public void VerifyCategoriaAlreadyExists(Category empresa)
        {
            ValidateCondition(empresa != null && empresa.ID != 0, "Nome já registrada.", 400);
        }

        public void VerifyUserIsMaster(int? userID)
        {
            ValidateCondition(userID == null || userID == 0 || userID != 4, "Usuário não autorizado.", 403);
        }

        public void VerifyProductIdIsPresent(int? productID)
        {
            ValidateId(productID, "Produto não encontrado.", 404);
        }

        public void VerifyProductAlreadyExists(Product product)
        {
            ValidateCondition(product != null && product.ID != 0, "Nome já registrado nesta categoria.", 400);
        }

        public void VerifyMesaAlreadyExists(Mesa mesa)
        {
            ValidateCondition(mesa != null && mesa.ID != 0, "Nome já registrado.", 400);
        }

        public void VerifyMesaAlreadyExists(QrCodeLayout qrCodeLayout)
        {
            ValidateCondition(qrCodeLayout != null && qrCodeLayout.ID != 0, "Nome já registrado.", 400);
        }

        public void VerifyGroupIdIsPresent(int? groupID)
        {
            ValidateId(groupID, "Grupo não encontrado.", 404);
        }

        public void VerifyCredentialsUser(User user, LoginModel loginModel)
        {
            ValidateCondition(
                user == null || loginModel == null || user.Email != loginModel.Login || user.Password != loginModel.Password,
                "Usuário ou credenciais inválidas.",
                401
            );
        }
    }
}
