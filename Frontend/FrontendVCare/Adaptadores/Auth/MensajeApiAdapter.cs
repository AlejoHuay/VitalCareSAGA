using System.Text.Json;
using FrontendVCare.Adaptadores;
using FrontendVCare.Dto;

namespace FrontendVCare.Adaptadores.Auth
{
    public class MensajeApiAdapter : AdapterJSON<MensajeApiDto>
    {
        public MensajeApiAdapter(HttpClient httpClient) : base(httpClient)
        {
        }

        public MensajeApiDto Adapt(JsonElement origen)
        {
            return new MensajeApiDto
            {
                Mensaje = origen.TryGetProperty("mensaje", out JsonElement mensajeElement)
                    ? mensajeElement.GetString() ?? string.Empty
                    : string.Empty
            };
        }

        public List<MensajeApiDto> AdaptList(IEnumerable<JsonElement> origen)
        {
            return origen.Select(Adapt).ToList();
        }
    }
}
