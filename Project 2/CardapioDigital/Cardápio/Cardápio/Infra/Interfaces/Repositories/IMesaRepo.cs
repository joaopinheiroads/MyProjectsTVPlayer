using Cardápio.Dto;
using Cardápio.Infra.Model;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IMesaRepo : IRepo<Mesa>
    {
        public Task<List<MesaGetDTO>> GetAllMesas(int empresaID);
        public Task<int> CountMesasAtivas(int empresaID);
    }
}
