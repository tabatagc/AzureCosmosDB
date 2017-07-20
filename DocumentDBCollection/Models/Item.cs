using Microsoft.Azure.Documents;
using Newtonsoft.Json;


namespace DocumentDBCollection.Models
{
    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "titulo")]
        public string Titulo { get; set; }

        [JsonProperty(PropertyName = "descricao")]
        public string Descricao { get; set; }

        [JsonProperty(PropertyName = "status")]
        public bool Status { get; set; }
    }
}