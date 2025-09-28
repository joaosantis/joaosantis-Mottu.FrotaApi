using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mottu.FrotaApi.Models
{
    public class Moto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Placa { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public int FilialId { get; set; }

        [JsonIgnore] 
        public Filial? Filial { get; set; }

        public ICollection<Manutencao> Manutencoes { get; set; } = new List<Manutencao>();
    }
}
