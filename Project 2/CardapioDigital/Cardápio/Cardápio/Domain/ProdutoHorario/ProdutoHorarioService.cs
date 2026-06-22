using Cardápio.Dto;
using Cardápio.Infra.Model;
using Cardápio.Infra.Repositories;

namespace Cardápio.Domain.ProdutoHorario
{
    public class ProdutoHorarioService
    {
        private readonly ProdutoHorarioRepo _produtoHorarioRepo;

        public ProdutoHorarioService(ProdutoHorarioRepo produtoHorarioRepo)
        {
            _produtoHorarioRepo = produtoHorarioRepo;
        }

        public async Task<List<ProdutoHorarioGetDTO>> GetHorariosByProdutoId(int produtoId)
        {
            var horarios = await _produtoHorarioRepo.GetHorariosByProdutoId(produtoId);
            return horarios.Select(h => new ProdutoHorarioGetDTO
            {
                ID = h.ID,
                ProdutoID = h.ProdutoID,
                HoraInicio = h.HoraInicio,
                HoraFim = h.HoraFim,
                DiaSemana = h.DiaSemana,
                Ativo = h.Ativo,
                DataCadastro = h.DataCadastro
            }).ToList();
        }

        public async Task<List<ProdutoHorarioGetDTO>> GetHorariosByEmpresa(int empresaId)
        {
            var horarios = await _produtoHorarioRepo.GetHorariosByEmpresa(empresaId);
            return horarios.Select(h => new ProdutoHorarioGetDTO
            {
                ID = h.ID,
                ProdutoID = h.ProdutoID,
                HoraInicio = h.HoraInicio,
                HoraFim = h.HoraFim,
                DiaSemana = h.DiaSemana,
                Ativo = h.Ativo,
                DataCadastro = h.DataCadastro
            }).ToList();
        }

        public async Task<ProdutoHorarioGetDTO?> GetById(int id)
        {
            var horario = await _produtoHorarioRepo.GetById(id);
            if (horario == null) return null;

            return new ProdutoHorarioGetDTO
            {
                ID = horario.ID,
                ProdutoID = horario.ProdutoID,
                HoraInicio = horario.HoraInicio,
                HoraFim = horario.HoraFim,
                DiaSemana = horario.DiaSemana,
                Ativo = horario.Ativo,
                DataCadastro = horario.DataCadastro
            };
        }

        public async Task<ProdutoHorarioGetDTO> Add(ProdutoHorarioAddDTO dto, int usuarioId)
        {
            if (!ValidarHorario(dto.HoraInicio, dto.HoraFim))
                throw new ArgumentException("Horário inválido. Hora de início deve ser menor que hora de fim.");

            var horario = new Infra.Model.ProdutoHorario
            {
                ProdutoID = dto.ProdutoID,
                HoraInicio = dto.HoraInicio,
                HoraFim = dto.HoraFim,
                DiaSemana = dto.DiaSemana,
                Ativo = dto.Ativo,
                DataCadastro = DateTime.Now,
                UsuarioIDCadastro = usuarioId
            };

            var result = await _produtoHorarioRepo.Add(horario);
            return new ProdutoHorarioGetDTO
            {
                ID = result.ID,
                ProdutoID = result.ProdutoID,
                HoraInicio = result.HoraInicio,
                HoraFim = result.HoraFim,
                DiaSemana = result.DiaSemana,
                Ativo = result.Ativo,
                DataCadastro = result.DataCadastro
            };
        }

        public async Task<ProdutoHorarioGetDTO> Update(ProdutoHorarioUpdateDTO dto, int usuarioId)
        {
            var horarioExistente = await _produtoHorarioRepo.GetById(dto.ID);
            if (horarioExistente == null)
                throw new ArgumentException("Horário não encontrado.");

            if (!ValidarHorario(dto.HoraInicio, dto.HoraFim))
                throw new ArgumentException("Horário inválido. Hora de início deve ser menor que hora de fim.");

            horarioExistente.HoraInicio = dto.HoraInicio;
            horarioExistente.HoraFim = dto.HoraFim;
            horarioExistente.DiaSemana = dto.DiaSemana;
            horarioExistente.Ativo = dto.Ativo;
            horarioExistente.DataEdicao = DateTime.Now;
            horarioExistente.UsuarioIDEdicao = usuarioId;

            var result = await _produtoHorarioRepo.Update(horarioExistente);
            return new ProdutoHorarioGetDTO
            {
                ID = result.ID,
                ProdutoID = result.ProdutoID,
                HoraInicio = result.HoraInicio,
                HoraFim = result.HoraFim,
                DiaSemana = result.DiaSemana,
                Ativo = result.Ativo,
                DataCadastro = result.DataCadastro
            };
        }

        public async Task<bool> Delete(int id)
        {
            return await _produtoHorarioRepo.Delete(id);
        }

        public async Task<bool> IsProdutoDisponivelNoHorario(int produtoId, DateTime? dataHora = null)
        {
            var agora = dataHora ?? DateTime.Now;
            return await _produtoHorarioRepo.IsProdutoDisponivelNoHorario(produtoId, agora);
        }

        public async Task<List<int>> GetProdutosDisponiveisNoHorario(List<int> produtoIds, DateTime? dataHora = null)
        {
            var agora = dataHora ?? DateTime.Now;
            var produtosDisponiveis = new List<int>();

            foreach (var produtoId in produtoIds)
            {
                if (await _produtoHorarioRepo.IsProdutoDisponivelNoHorario(produtoId, agora))
                {
                    produtosDisponiveis.Add(produtoId);
                }
            }

            return produtosDisponiveis;
        }

        private bool ValidarHorario(string horaInicio, string horaFim)
        {
            if (TimeSpan.TryParse(horaInicio, out var inicio) && 
                TimeSpan.TryParse(horaFim, out var fim))
            {
                return inicio < fim;
            }
            return false;
        }
    }
}
