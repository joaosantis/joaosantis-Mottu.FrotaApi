using System.ComponentModel.DataAnnotations;

namespace Mottu.FrotaApi.Models;

public class Moto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    [RegularExpression(@"^[A-Z]{3}[0-9][0-9A-Z][0-9]{2}$", 
        ErrorMessage = "Placa inválida (padrão brasileiro).")]
    public string Placa { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Modelo { get; set; } = default!;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = default!;

    public int FilialId { get; set; }
    public Filial Filial { get; set; } = default!;
}
