using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TVPlayerSite.API.Interfaces.UnitOfWork;
using TVPlayer.CRUD.Models;
using Microsoft.Extensions.Logging;
using TVPlayerSite.Models.Video;
using Microsoft.EntityFrameworkCore;



namespace TVPlayerSite.API.Repositories.tvplayer02Repositories
{
    public class LeadRepository : ILeadRepository
    {
        private readonly VideoContext _context;
        private readonly ILogger<LeadRepository> _logger;


        public LeadRepository(VideoContext context, ILogger<LeadRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SalvarAsync(Leads lead)
        {
            var existe = await _context.Leads.AnyAsync(l => l.LeadId == lead.LeadId);

            if (existe)
            {
                _logger.LogWarning($"⚠️ Lead {lead.LeadId} já existe no banco. Ignorando duplicata.");
                return;
            }

            _logger.LogInformation($"💾 Salvando lead {lead.LeadId} - {lead.Nome} no banco...");
            await _context.Leads.AddAsync(lead);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"✅ Lead {lead.LeadId} salvo com sucesso! (ID interno: {lead.Id})");
        }
    }
}
