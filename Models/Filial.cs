using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mottu.FrotaApi.Models
{
    public class Filial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "NUMBER")]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "NVARCHAR2(200)")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "NVARCHAR2(300)")]
        public string Endereco { get; set; } = string.Empty;

        public ICollection<Moto> Motos { get; set; } = new List<Moto>();
    }
}
