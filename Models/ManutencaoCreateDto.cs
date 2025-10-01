using System.ComponentModel.DataAnnotations;

namespace Mottu.FrotaApi.Models
{
    public class ManutencaoCreateDto
    {
        [Required, MaxLength(500)]
        public string Descricao { get; set; } = default!;

        [Required]
        public DateTime Data { get; set; }  // datetime2 no SQL Server

        [Required]
        public int MotoId { get; set; }     // referência à Moto
    }
}
