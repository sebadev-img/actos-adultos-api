using ActosAdultosAPI.DTO;
using ActosAdultosAPI.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ActosAdultosAPI.Services.Implementation
{
    public class JuntaApiService: IJuntaApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "Junta_Codigos";

        public JuntaApiService(
            HttpClient httpClient,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<List<CodigoJuntaDto>> GetCodigosJuntaAsync(
            CancellationToken cancellationToken = default)
        {
            // 1. Intentar recuperar los datos del caché
            if (_cache.TryGetValue(
                CacheKey,
                out List<CodigoJuntaDto>? cachedCodigos) &&
                cachedCodigos != null)
            {
                return cachedCodigos;
            }

            // 2. Si no hay caché, consultar la API externa
            using var response = await _httpClient.GetAsync(
                "inscripcion/apis/consulta.php?opc=Listado",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content
                .ReadAsStringAsync(cancellationToken);

            var codigosExternos = JsonConvert.DeserializeObject<
                List<CodigoJuntaExternoDto>>(content);

            if (codigosExternos == null)
            {
                throw new JsonSerializationException(
                    "La API de Junta no devolvió un listado válido.");
            }

            var codigos = codigosExternos
                .Select(c => new CodigoJuntaDto
                {
                    Codigo = c.Codigo,
                    Descripcion = c.Descripcion
                })
                .ToList();

            // 3. Guardar el listado durante 30 minutos
            _cache.Set(
                CacheKey,
                codigos,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(30)
                });

            return codigos;
        }

        private class CodigoJuntaExternoDto
        {
            [JsonProperty("codEspacio")]
            public string Codigo { get; set; } = string.Empty;

            [JsonProperty("desEspacio")]
            public string Descripcion { get; set; } = string.Empty;
        }


        public async Task<JuntaConsultaDto?> ConsultarPuntajeAsync(
            long dni,
            string codigoJunta,
            string ciudad,
            CancellationToken cancellationToken = default)
        {
            var query =
                "inscripcion/apis/consulta.php" +
                $"?doc={Uri.EscapeDataString(dni.ToString(
                    System.Globalization.CultureInfo.InvariantCulture))}" +
                $"&cargo={Uri.EscapeDataString(codigoJunta)}" +
                $"&ciudad={Uri.EscapeDataString(ciudad)}" +
                "&opc=consulta";

            using var response = await _httpClient.GetAsync(
                query, cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content
                .ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
                return null;

            return JsonConvert.DeserializeObject<JuntaConsultaDto>(
                content);
        }

    }
}
