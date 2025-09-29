using System.ComponentModel.DataAnnotations;

namespace Mottu.FrotaApi.Models
{
    public class Moto
    {
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string Placa { get; set; } = "";

        [Required, StringLength(100)]
        public string Modelo { get; set; } = "";

        public int Ano { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "ATIVA";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
