using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IRepo<T> where T : class
    {
        Task AddAsync(T entity);
        Task<T> GetByIDAsync(int ID, int empresaID);
        Task UpdateAsync(T entity);
    }
}