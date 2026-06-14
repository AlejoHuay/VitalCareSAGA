using FrontendVCare.Helpers;

namespace FrontendVCare.Servicios
{
    /// <summary>
    /// Manejador HTTP que automáticamente inyecta el token JWT en todas las peticiones
    /// </summary>
    public class JwtTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? token = JwtSessionHelper.ObtenerToken(httpContext);
                
                Console.WriteLine(
                    string.IsNullOrWhiteSpace(token)
                        ? "JWT NO ENCONTRADO EN LA SESION"
                        : "JWT ENCONTRADO Y AGREGADO A LA PETICION"
                );
                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
