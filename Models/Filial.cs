using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// Se quiser cortar o retorno da coleção no JSON, descomente a linha abaixo e marque a propriedade com [JsonIgnore].
// using System.Text.Json.Serialization;

namespace Mottu.FrotaApi.Models
{
    public class Filial
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Nome { get; set; } = default!;

        [MaxLength(300)]
        public string Endereco { get; set; } = default!;

        // coleção de motos da filial
        // Se quiser evitar retornar a lista de motos junto com a Filial, adicione [JsonIgnore] aqui.
        public ICollection<Moto> Motos { get; set; } = new List<Moto>();
    }
}
