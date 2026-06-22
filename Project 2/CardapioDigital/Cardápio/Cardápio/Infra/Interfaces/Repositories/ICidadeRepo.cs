using Cardápio.Dto;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface ICidadeRepo
    {
        Task<IEnumerable<CidadeGetDTO>> GetCidadeByEstadoIDAsync(string estadoID);
        Task<CidadeGetDTO> GetCidadeByIDAsync(int cidadeID, int empresaID);
    }
}
