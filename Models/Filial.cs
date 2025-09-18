namespace Mottu.FrotaApi.Models
{
    public class Filial
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;

        
        public ICollection<Moto> Motos { get; set; } = new List<Moto>();

    
    }
}
