using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerSite.API.DTO;
using System.Threading.Tasks;
using TVPlayerSite.API.DTO;



namespace TVPlayerSite.API.Interfaces.UnitOfWork
{
   
        public interface IKommoService
        {
            Task MoverLeadAsync(string leadId, long statusId);
            Task ProcessarAsync(GrupoAtualizadoDTO dto);
            Task ProcessarWebhookAsync(WebhookPayloadDTO payload);



    }

       
}

