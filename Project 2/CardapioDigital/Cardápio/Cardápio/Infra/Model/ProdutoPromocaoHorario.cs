using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardápio.Infra.Model
{
    /// <summary>
    /// Entidade para promoções por produto, configuradas por dia da semana e horário
    /// Permite que cada produto tenha promoções específicas para cada dia e horário
    /// </summary>
    [Table("ProdutoPromocaoHorario")]
    public class ProdutoPromocaoHorario
    {
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// ID do produto que terá a promoção
        /// </summary>
        [Required]
        public int ProdutoID { get; set; }

        /// <summary>
        /// Produto relacionado à promoção
        /// </summary>
        [ForeignKey(nameof(ProdutoID))]
        public Product? Produto { get; set; }

        /// <summary>
        /// Dia da semana: Segunda, Terça, Quarta, Quinta, Sexta, Sábado, Domingo
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string DiaSemana { get; set; } = string.Empty;

        /// <summary>
        /// Hora de início da promoção (armazenada como string no banco)
        /// </summary>
        [Required]
        [MaxLength(8)]
        [Column("HoraInicio")]
        public string HoraInicioString { get; set; } = string.Empty;

        /// <summary>
        /// Hora de fim da promoção (armazenada como string no banco)
        /// </summary>
        [Required]
        [MaxLength(8)]
        [Column("HoraFim")]
        public string HoraFimString { get; set; } = string.Empty;

        /// <summary>
        /// Propriedade auxiliar para conversão de HoraInicio para TimeSpan
        /// </summary>
        [NotMapped]
        public TimeSpan HoraInicio 
        { 
            get => TryParseTimeSpan(HoraInicioString); 
            set => HoraInicioString = value.ToString(@"hh\:mm\:ss"); 
        }

        /// <summary>
        /// Propriedade auxiliar para conversão de HoraFim para TimeSpan
        /// </summary>
        [NotMapped]
        public TimeSpan HoraFim 
        { 
            get => TryParseTimeSpan(HoraFimString); 
            set => HoraFimString = value.ToString(@"hh\:mm\:ss"); 
        }

        /// <summary>
        /// Preço promocional do produto durante este horário
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(19,2)")]
        public decimal PrecoPromocional { get; set; }

        /// <summary>
        /// Se a promoção está ativa
        /// </summary>
        [Required]
        public bool Ativo { get; set; } = true;

        /// <summary>
        /// Data de cadastro da promoção
        /// </summary>
        [Required]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        /// <summary>
        /// Data de última edição da promoção
        /// </summary>
        public DateTime? DataEdicao { get; set; }

        /// <summary>
        /// ID do usuário que cadastrou a promoção
        /// </summary>
        [Required]
        public int UsuarioIDCadastro { get; set; }

        /// <summary>
        /// ID do usuário que editou a promoção
        /// </summary>
        public int? UsuarioIDEdicao { get; set; }

        /// <summary>
        /// Método auxiliar para converter string para TimeSpan de forma robusta
        /// </summary>
        private static TimeSpan TryParseTimeSpan(string timeString)
        {
            if (string.IsNullOrEmpty(timeString))
                return TimeSpan.Zero;

            try
            {
                timeString = timeString.Trim();
                
                if (TimeSpan.TryParseExact(timeString, @"hh\:mm\:ss", null, out TimeSpan result1))
                    return result1;
                
                if (TimeSpan.TryParseExact(timeString, @"h\:mm\:ss", null, out TimeSpan result2))
                    return result2;
                    
                if (TimeSpan.TryParseExact(timeString, @"hh\:mm", null, out TimeSpan result3))
                    return result3;
                    
                if (TimeSpan.TryParseExact(timeString, @"h\:mm", null, out TimeSpan result4))
                    return result4;
                
                if (timeString.Length == 5 && timeString.Contains(':'))
                {
                    timeString += ":00";
                    if (TimeSpan.TryParseExact(timeString, @"hh\:mm\:ss", null, out TimeSpan result5))
                        return result5;
                }
                
                if (TimeSpan.TryParse(timeString, out TimeSpan result6))
                    return result6;
            }
            catch
            {
                // Em caso de erro, retorna zero
            }

            return TimeSpan.Zero;
        }
    }
}
