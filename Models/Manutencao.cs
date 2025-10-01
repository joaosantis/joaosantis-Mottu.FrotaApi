using System.ComponentModel.DataAnnotations;

namespace Mottu.FrotaApi.Models;

public class Manutencao
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Descricao { get; set; } = default!;

    [Required]
    [DataType(DataType.Date)]
    [CustomValidation(typeof(Manutencao), nameof(ValidarData))]
    public DateTime Data { get; set; }

    public int MotoId { get; set; }
    public Moto Moto { get; set; } = default!;

    // 🔹 Validação customizada
    public static ValidationResult? ValidarData(DateTime data, ValidationContext context)
    {
        if (data > DateTime.Now)
            return new ValidationResult("Data de manutenção não pode ser no futuro.");
        return ValidationResult.Success;
    }
}
