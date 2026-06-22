using Cardápio.Infra.Interfaces.Repositories;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IProdutoPromocaoHorarioRepo : IRepo<ProdutoPromocaoHorario>
    {
        Task<List<ProdutoPromocaoHorario>> GetPromocoesByProdutoIdAsync(int produtoId);
       /* Task<List<ProdutoPromocaoHorario>> GetAllPromocoesByProdutoIdAsync(int produtoId);*/
        Task<ProdutoPromocaoHorario?> GetPromocaoAtivaAsync(int produtoId, string diaSemana, TimeSpan horaAtual);
        Task<List<ProdutoPromocaoHorario>> GetPromocoesAtivasAsync();
        Task<List<ProdutoPromocaoHorario>> GetPromocoesByDiaSemanaAsync(string diaSemana);
        Task<bool> VerificarConflitosHorarioAsync(int produtoId, string diaSemana, string horaInicio, string horaFim, int? promocaoId = null);
        Task DesativarPromocoesProdutoAsync(int produtoId);
        Task<bool> DesativarPromocaoAsync(int promocaoId, int usuarioId); // ← NOVO método para desativar promoção específica
    }
}
