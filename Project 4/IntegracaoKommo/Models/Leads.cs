using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TVPlayerSite.Models.Video
{
     [Table("Leads")]
        public class Leads
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            [Required]
            public string LeadId { get; set; }

            public string ContactId { get; set; }

            public string Nome { get; set; }

            public string Email { get; set; }

            public string Telefone { get; set; }

            public DateTime CriadoEm { get; set; } = DateTime.Now;
        }

    }
