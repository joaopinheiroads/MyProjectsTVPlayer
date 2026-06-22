using Cardápio.Dto;
using Cardápio.Infra.Model;
using Cardápio.Infra.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Cardápio.Domain.Promocao
{
    public class PromocaoService
    {
        private readonly PromocaoRepo _promocaoRepo;
        private readonly PromocaoHorarioRepo _promocaoHorarioRepo;
        private readonly ProdutoPromocaoRepo _produtoPromocaoRepo;

        public PromocaoService(
            PromocaoRepo promocaoRepo, 
            PromocaoHorarioRepo promocaoHorarioRepo,
            ProdutoPromocaoRepo produtoPromocaoRepo)
        {
            _promocaoRepo = promocaoRepo;
            _promocaoHorarioRepo = promocaoHorarioRepo;
            _produtoPromocaoRepo = produtoPromocaoRepo;
        }

        public async Task<List<PromocaoGetDTO>> GetPromocoesByEmpresa(int empresaId)
        {
            var promocoes = await _promocaoRepo.GetPromocoesByEmpresa(empresaId);
            return promocoes.Select(p => MapToDTO(p)).ToList();
        }

        public async Task<PromocaoGetDTO?> GetById(int id)
        {
            var promocao = await _promocaoRepo.GetById(id);
            return promocao != null ? MapToDTO(promocao) : null;
        }

        public async Task<PromocaoGetDTO> Add(PromocaoAddDTO dto, int usuarioId)
        {
            var promocao = new Infra.Model.Promocao
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                Ativo = dto.Ativo,
                EmpresaID = dto.EmpresaID,
                DataCadastro = DateTime.Now,
                UsuarioIDCadastro = usuarioId
            };

            var savedPromocao = await _promocaoRepo.Add(promocao);

            // Adicionar horários
            foreach (var horarioDto in dto.Horarios)
            {
                var horario = new Cardápio.Infra.Model.PromocaoHorario
                {
                    PromocaoID = savedPromocao.ID,
                    HoraInicio = horarioDto.HoraInicio,
                    HoraFim = horarioDto.HoraFim,
                    DiaSemana = horarioDto.DiaSemana,
                    Ativo = horarioDto.Ativo,
                    DataCadastro = DateTime.Now,
                    UsuarioIDCadastro = usuarioId
                };
                await _promocaoHorarioRepo.Add(horario);
            }

            // Adicionar produtos
            foreach (var produtoDto in dto.Produtos)
            {
                var produtoPromocao = new ProdutoPromocao
                {
                    PromocaoID = savedPromocao.ID,
                    ProdutoID = produtoDto.ProdutoID,
                    PrecoPromocional = produtoDto.PrecoPromocional,
                    Ativo = produtoDto.Ativo,
                    DataCadastro = DateTime.Now,
                    UsuarioIDCadastro = usuarioId
                };
                await _produtoPromocaoRepo.Add(produtoPromocao);
            }

            // Retornar a promoção completa
            var promocaoCompleta = await _promocaoRepo.GetById(savedPromocao.ID);
            return MapToDTO(promocaoCompleta!);
        }

        public async Task<PromocaoGetDTO> Update(PromocaoUpdateDTO dto, int usuarioId)
        {
            var promocao = await _promocaoRepo.GetById(dto.ID);
            if (promocao == null)
                throw new ArgumentException("Promoção não encontrada");

            promocao.Nome = dto.Nome;
            promocao.Descricao = dto.Descricao;
            promocao.DataInicio = dto.DataInicio;
            promocao.DataFim = dto.DataFim;
            promocao.Ativo = dto.Ativo;
            promocao.DataEdicao = DateTime.Now;
            promocao.UsuarioIDEdicao = usuarioId;

            await _promocaoRepo.Update(promocao);

            // Atualizar horários (desativar todos e criar novos)
            var horariosExistentes = await _promocaoHorarioRepo.GetHorariosByPromocaoId(dto.ID);
            foreach (var horario in horariosExistentes)
            {
                await _promocaoHorarioRepo.Delete(horario.ID);
            }

            foreach (var horarioDto in dto.Horarios)
            {
                var horario = new Cardápio.Infra.Model.PromocaoHorario
                {
                    PromocaoID = dto.ID,
                    HoraInicio = horarioDto.HoraInicio,
                    HoraFim = horarioDto.HoraFim,
                    DiaSemana = horarioDto.DiaSemana,
                    Ativo = horarioDto.Ativo,
                    DataCadastro = DateTime.Now,
                    UsuarioIDCadastro = usuarioId
                };
                await _promocaoHorarioRepo.Add(horario);
            }

            // Atualizar produtos (desativar todos e criar novos)
            var produtosExistentes = await _produtoPromocaoRepo.GetByPromocaoId(dto.ID);
            foreach (var produto in produtosExistentes)
            {
                await _produtoPromocaoRepo.Delete(produto.ID);
            }

            foreach (var produtoDto in dto.Produtos)
            {
                var produtoPromocao = new ProdutoPromocao
                {
                    PromocaoID = dto.ID,
                    ProdutoID = produtoDto.ProdutoID,
                    PrecoPromocional = produtoDto.PrecoPromocional,
                    Ativo = produtoDto.Ativo,
                    DataCadastro = DateTime.Now,
                    UsuarioIDCadastro = usuarioId
                };
                await _produtoPromocaoRepo.Add(produtoPromocao);
            }

            // Retornar a promoção atualizada
            var promocaoAtualizada = await _promocaoRepo.GetById(dto.ID);
            return MapToDTO(promocaoAtualizada!);
        }

        public async Task<bool> Delete(int id)
        {
            return await _promocaoRepo.Delete(id);
        }

        public async Task<List<PromocaoGetDTO>> GetPromocoesAtivas(int empresaId)
        {
            var dataAtual = DateTime.Now;
            var promocoes = await _promocaoRepo.GetPromocoesAtivas(empresaId, dataAtual);
            return promocoes.Select(p => MapToDTO(p)).ToList();
        }

        public async Task<decimal?> GetPrecoPromocionalProduto(int produtoId)
        {
            var dataAtual = DateTime.Now;
            var diaSemana = GetDiaSemanaPortugues(dataAtual.DayOfWeek);
            var horaAtual = dataAtual.ToString("HH:mm");

            var promocoesAtivas = await _produtoPromocaoRepo.GetPromocoesAtivasPorProduto(
                produtoId, dataAtual, diaSemana, horaAtual);

            // Retorna o menor preço promocional ativo no momento
            return promocoesAtivas.Any() ? promocoesAtivas.Min(p => p.PrecoPromocional) : null;
        }

        private static string GetDiaSemanaPortugues(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Segunda",
                DayOfWeek.Tuesday => "Terça",
                DayOfWeek.Wednesday => "Quarta",
                DayOfWeek.Thursday => "Quinta",
                DayOfWeek.Friday => "Sexta",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => ""
            };
        }

        private static PromocaoGetDTO MapToDTO(Infra.Model.Promocao promocao)
        {
            return new PromocaoGetDTO
            {
                ID = promocao.ID,
                Nome = promocao.Nome,
                Descricao = promocao.Descricao,
                DataInicio = promocao.DataInicio,
                DataFim = promocao.DataFim,
                Ativo = promocao.Ativo,
                EmpresaID = promocao.EmpresaID ?? 0,
                DataCadastro = promocao.DataCadastro,
                DataEdicao = promocao.DataEdicao,
                UsuarioIDCadastro = promocao.UsuarioIDCadastro ?? 0,
                UsuarioIDEdicao = promocao.UsuarioIDEdicao,
                Horarios = promocao.PromocaoHorarios.Select(h => new PromocaoHorarioGetDTO
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
                }).ToList(),
                Produtos = promocao.ProdutoPromocoes.Select(pp => new ProdutoPromocaoGetDTO
                {
                    ID = pp.ID,
                    ProdutoID = pp.ProdutoID,
                    PromocaoID = pp.PromocaoID,
                    PrecoPromocional = pp.PrecoPromocional,
                    Ativo = pp.Ativo,
                    DataCadastro = pp.DataCadastro,
                    DataEdicao = pp.DataEdicao,
                    UsuarioIDCadastro = pp.UsuarioIDCadastro,
                    UsuarioIDEdicao = pp.UsuarioIDEdicao,
                    ProdutoNome = pp.Produto?.Nome,
                    PromocaoNome = pp.Promocao?.Nome
                }).ToList()
            };
        }
    }
}
