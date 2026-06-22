using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerSite.Models.Video;

namespace TVPlayerSite.API.Interfaces.UnitOfWork
{
    public interface ILeadRepository
    {
        Task SalvarAsync(Leads lead);
    }
}
