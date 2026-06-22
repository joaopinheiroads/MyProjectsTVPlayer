using Cardápio.Dto;
using Cardápio.Infra.Model;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IProdutoImagemRepo : IRepo<ImageProduct>
    {
        Task<ImageProduct> GetProdutoImagemByProdutoIDAsync(int produtoID);
    }
}
