using Newtonsoft.Json;

namespace ActosAdultosAPI.DTO
{
    public class JuntaConsultaDto
    {
        [JsonProperty("dni")]
        public string? Dni { get; set; }

        [JsonProperty("cod_espacio")]
        public string? CodigoEspacio { get; set; }

        [JsonProperty("caracter")]
        public string? Caracter { get; set; }

        [JsonProperty("puntaje")]
        public string? Puntaje { get; set; }

        [JsonProperty("obs")]
        public string? Observaciones { get; set; }
    }
}
