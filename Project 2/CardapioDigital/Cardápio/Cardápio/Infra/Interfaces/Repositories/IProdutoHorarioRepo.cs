using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IProdutoHorarioRepo
    {

        Task<List<ProdutoHorario>> GetHorariosByProdutoId(int produtoId);
        Task<List<ProdutoHorario>> GetHorariosByEmpresa(int empresaId);
        Task<ProdutoHorario?> GetById(int id);
        Task<ProdutoHorario> Add(ProdutoHorario produtoHorario);
        Task<ProdutoHorario> Update(ProdutoHorario produtoHorario);
        Task<bool> Delete(int id);
        Task<bool> IsProdutoDisponivelNoHorario(int produtoId, DateTime agora);


    }
}