namespace Mottu.FrotaApi.Models
{
    public class Manutencao
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public DateTime Data { get; set; }

        public int MotoId { get; set; }
        public Moto Moto { get; set; } = null!;

        
        public Filial Filial => Moto.Filial;
    }
}