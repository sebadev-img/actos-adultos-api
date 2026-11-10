using System.Text.Json.Serialization;

namespace ActosAdultosAPI.DTO
{
    public class CodigoJuntaDto
    {
        [JsonPropertyName("codEspacio")]
        public string Codigo { get; set; } = string.Empty;

        [JsonPropertyName("desEspacio")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
