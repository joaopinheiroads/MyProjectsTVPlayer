using Cardápio.Infra.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardápio.Infra.Interfaces.Repositories
{
    public interface IGrupoUsuarioRepo : IRepo<UserGroup>
    {
        Task<UserGroup> GetGroupByUserID(int usuarioID);
    }
}
