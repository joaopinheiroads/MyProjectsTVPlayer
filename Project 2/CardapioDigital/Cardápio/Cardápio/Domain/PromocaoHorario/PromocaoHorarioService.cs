using Cardápio.Dto;
using Cardápio.Infra.Model;
using Cardápio.Infra.Repositories;

namespace Cardápio.Domain.PromocaoHorario
{
    public class PromocaoHorarioService
    {
        private readonly PromocaoHorarioRepo _promocaoHorarioRepo;

        public PromocaoHorarioService(PromocaoHorarioRepo promocaoHorarioRepo)
        {
            _promocaoHorarioRepo = promocaoHorarioRepo;
        }

        public async Task<List<PromocaoHorarioGetDTO>> GetHorariosByPromocaoId(int promocaoId)
        {
            var horarios = await _promocaoHorarioRepo.GetHorariosByPromocaoId(promocaoId);
            return horarios.Select(h => new PromocaoHorarioGetDTO
            {
                ID = h.ID,
                PromocaoID = h.PromocaoID,
                HoraInicio = h.HoraInicio,
                HoraFim = h.HoraFim,
                DiaSemana = h.DiaSemana,
                Ativo = h.Ativo,
                DataCadastro = h.DataCadastro,
                DataEdicao = h.DataEdicao,
                UsuarioIDCadastro = h.UsuarioIDCadastro,
                UsuarioIDEdicao = h.UsuarioIDEdicao
            }).ToList();
        }

        public async Task<List<PromocaoHorarioGetDTO>> GetHorariosByEmpresa(int empresaId)
        {
            var horarios = await _promocaoHorarioRepo.GetHorariosByEmpresa(empresaId);
            return horarios.Select(h => new PromocaoHorarioGetDTO
            {
                ID = h.ID,
                PromocaoID = h.PromocaoID,
                HoraInicio = h.HoraInicio,
                HoraFim = h.HoraFim,
                DiaSemana = h.DiaSemana,
                Ativo = h.Ativo,
                DataCadastro = h.DataCadastro,
                DataEdicao = h.DataEdicao,
                UsuarioIDCadastro = h.UsuarioIDCadastro,
                UsuarioIDEdicao = h.UsuarioIDEdicao
            }).ToList();
        }

        public async Task<PromocaoHorarioGetDTO?> GetById(int id)
        {
            var horario = await _promocaoHorarioRepo.GetById(id);
            if (horario == null) return null;

            return new PromocaoHorarioGetDTO
            {
                ID = horario.ID,
                PromocaoID = horario.PromocaoID,
                HoraInicio = horario.HoraInicio,
                HoraFim = horario.HoraFim,
                DiaSemana = horario.DiaSemana,
                Ativo = horario.Ativo,
                DataCadastro = horario.DataCadastro,
                DataEdicao = horario.DataEdicao,
                UsuarioIDCadastro = horario.UsuarioIDCadastro,
                UsuarioIDEdicao = horario.UsuarioIDEdicao
            };
        }

        public async Task<PromocaoHorarioGetDTO> Add(PromocaoHorarioAddDTO dto, int usuarioId)
        {
            var horario = new Cardápio.Infra.Model.PromocaoHorario
            {
                PromocaoID = dto.PromocaoID,
                HoraInicio = dto.HoraInicio,
                HoraFim = dto.HoraFim,
                DiaSemana = dto.DiaSemana,
                Ativo = dto.Ativo,
                DataCadastro = DateTime.Now,
                UsuarioIDCadastro = usuarioId
            };

            var savedHorario = await _promocaoHorarioRepo.Add(horario);

            return new PromocaoHorarioGetDTO
            {
                ID = savedHorario.ID,
                PromocaoID = savedHorario.PromocaoID,
                HoraInicio = savedHorario.HoraInicio,
                HoraFim = savedHorario.HoraFim,
                DiaSemana = savedHorario.DiaSemana,
                Ativo = savedHorario.Ativo,
                DataCadastro = savedHorario.DataCadastro,
                DataEdicao = savedHorario.DataEdicao,
                UsuarioIDCadastro = savedHorario.UsuarioIDCadastro,
                UsuarioIDEdicao = savedHorario.UsuarioIDEdicao
            };
        }

        public async Task<PromocaoHorarioGetDTO> Update(PromocaoHorarioUpdateDTO dto, int usuarioId)
        {
            var horario = await _promocaoHorarioRepo.GetById(dto.ID);
            if (horario == null)
                throw new ArgumentException("Horário de promoção não encontrado");

            horario.PromocaoID = dto.PromocaoID;
            horario.HoraInicio = dto.HoraInicio;
            horario.HoraFim = dto.HoraFim;
            horario.DiaSemana = dto.DiaSemana;
            horario.Ativo = dto.Ativo;
            horario.DataEdicao = DateTime.Now;
            horario.UsuarioIDEdicao = usuarioId;

            var updatedHorario = await _promocaoHorarioRepo.Update(horario);

            return new PromocaoHorarioGetDTO
            {
                ID = updatedHorario.ID,
                PromocaoID = updatedHorario.PromocaoID,
                HoraInicio = updatedHorario.HoraInicio,
                HoraFim = updatedHorario.HoraFim,
                DiaSemana = updatedHorario.DiaSemana,
                Ativo = updatedHorario.Ativo,
                DataCadastro = updatedHorario.DataCadastro,
                DataEdicao = updatedHorario.DataEdicao,
                UsuarioIDCadastro = updatedHorario.UsuarioIDCadastro,
                UsuarioIDEdicao = updatedHorario.UsuarioIDEdicao
            };
        }

        public async Task<bool> Delete(int id)
        {
            return await _promocaoHorarioRepo.Delete(id);
        }

        public async Task<List<PromocaoHorarioGetDTO>> GetHorariosAtivos(DateTime dataAtual, string diaSemana, string horaAtual)
        {
            var horarios = await _promocaoHorarioRepo.GetHorariosAtivos(dataAtual, diaSemana, horaAtual);
            return horarios.Select(h => new PromocaoHorarioGetDTO
            {
                ID = h.ID,
                PromocaoID = h.PromocaoID,
                HoraInicio = h.HoraInicio,
                HoraFim = h.HoraFim,
                DiaSemana = h.DiaSemana,
                Ativo = h.Ativo,
                DataCadastro = h.DataCadastro,
                DataEdicao = h.DataEdicao,
                UsuarioIDCadastro = h.UsuarioIDCadastro,
                UsuarioIDEdicao = h.UsuarioIDEdicao
            }).ToList();
        }
    }
}
