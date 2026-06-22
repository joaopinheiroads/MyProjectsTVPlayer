using Cardápio.Infra.Model;
using Cardápio.Infra.Repositories;

namespace Cardápio.Domain.ProdutoPromocaoHorario
{
    /// <summary>
    /// Serviço para gerenciar promoções por produto, dia da semana e horário
    /// </summary>
    public class ProdutoPromocaoHorarioService
    {
        private readonly ProdutoPromocaoHorarioRepo _produtoPromocaoHorarioRepo;

        public ProdutoPromocaoHorarioService(ProdutoPromocaoHorarioRepo produtoPromocaoHorarioRepo)
        {
            _produtoPromocaoHorarioRepo = produtoPromocaoHorarioRepo;
        }

        /// <summary>
        /// Verifica se produto tem promoção ativa no horário atual e retorna o preço
        /// </summary>
        public async Task<decimal?> GetPrecoPromocionalAtual(int produtoId, DateTime? dataHora = null)
        {
            // Usar horário local do Brasil para teste
            var agora = dataHora ?? DateTime.Now;
            var diaSemana = GetDiaSemanaPortugues(agora.DayOfWeek);
            var horaAtual = agora.ToString("HH:mm");

            var precoPromocional = await _produtoPromocaoHorarioRepo.GetPrecoPromocionalAtivo(
                produtoId, diaSemana, horaAtual);

            return precoPromocional;
        }

        /// <summary>
        /// Desativa uma promoção específica por ID
        /// </summary>
        public async Task<bool> DesativarPromocaoAsync(int promocaoId, int usuarioId)
        {
            try
            {
                
                var resultado = await _produtoPromocaoHorarioRepo.DesativarPromocaoAsync(promocaoId, usuarioId);
                
                if (resultado)
                {
                    Console.WriteLine($"[ProdutoPromocaoHorarioService] ✅ Promoção {promocaoId} desativada com sucesso");
                }
                else
                {
                    Console.WriteLine($"[ProdutoPromocaoHorarioService] ❌ Falha ao desativar promoção {promocaoId} - promoção não encontrada ou já inativa");
                }
                
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProdutoPromocaoHorarioService] ❌ Erro ao desativar promoção {promocaoId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converte DayOfWeek do .NET para o formato usado no banco (português)
        /// </summary>
        private string GetDiaSemanaPortugues(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "Domingo",
                DayOfWeek.Monday => "Segunda",
                DayOfWeek.Tuesday => "Terça",
                DayOfWeek.Wednesday => "Quarta",
                DayOfWeek.Thursday => "Quinta",
                DayOfWeek.Friday => "Sexta",
                DayOfWeek.Saturday => "Sábado",
                _ => "Segunda"
            };
        }
    }
}
