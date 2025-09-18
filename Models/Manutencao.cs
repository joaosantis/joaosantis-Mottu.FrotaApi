using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Mottu.FrotaApi.Models
{
    public class Manutencao
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "NUMBER")]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR2(500)")]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "DATE")]
        public DateTime Data { get; set; }

        [Required]
        [Column(TypeName = "NUMBER")]
        public int MotoId { get; set; }

        [JsonIgnore] 
        public Moto? Moto { get; set; }
    }
}
