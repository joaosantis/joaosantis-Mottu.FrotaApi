namespace Mottu.FrotaApi.Models
{
    public class Moto
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;


        public int FilialId { get; set; }
        public Filial Filial { get; set; } = null!; 

        
        public ICollection<Manutencao> Manutencoes { get; set; } = new List<Manutencao>();
    }
}
