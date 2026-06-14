using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.Entidades;

namespace MSReportes.API.FrameworksYDrivers.Repositorios
{
    public class ReporteVentasRepositorio : IReporteVentasRepositorio
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IHttpContextAccessor httpContextAccessor;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReporteVentasRepositorio(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpClientFactory = httpClientFactory;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            List<VentaReporteDto> ventas = await ObtenerVentasConfirmadasAsync(desde, hasta);
            Dictionary<int, string> rolesPorUsuario = new();
            Dictionary<string, ReporteVentasPorRolDto> acumulado = new(StringComparer.OrdinalIgnoreCase);

            foreach (VentaReporteDto venta in ventas)
            {
                if (!rolesPorUsuario.TryGetValue(venta.IdUsuario, out string? rol))
                {
                    rol = await ObtenerRolUsuarioAsync(venta.IdUsuario);
                    rolesPorUsuario[venta.IdUsuario] = rol;
                }

                if (!acumulado.TryGetValue(rol, out ReporteVentasPorRolDto? item))
                {
                    item = new ReporteVentasPorRolDto { Rol = rol };
                    acumulado[rol] = item;
                }

                item.CantidadVentas++;
                item.TotalRecaudado += venta.Total;
            }

            return acumulado.Values
                .OrderByDescending(item => item.CantidadVentas)
                .ThenByDescending(item => item.TotalRecaudado)
                .ThenBy(item => item.Rol)
                .ToList();
        }

        public async Task<ComprobanteVentaDto?> ObtenerComprobanteVentaAsync(int idVenta)
        {
            HttpClient ventasClient = CrearClienteAutenticado("MSVentas");
            HttpResponseMessage ventaResponse = await ventasClient.GetAsync($"api/ventas/{idVenta}");

            if (!ventaResponse.IsSuccessStatusCode)
                return null;

            RespuestaVentaDto? respuestaVenta =
                await ventaResponse.Content.ReadFromJsonAsync<RespuestaVentaDto>(JsonOptions);

            VentaReporteDto? venta = respuestaVenta?.Data;
            if (venta == null)
                return null;

            ComprobanteVentaDto comprobante = new ComprobanteVentaDto
            {
                IdVenta = venta.Id,
                Fecha = venta.FechaHora != default ? venta.FechaHora : venta.Fecha,
                Nit = string.IsNullOrWhiteSpace(venta.Nit) ? "-" : venta.Nit,
                RazonSocial = string.IsNullOrWhiteSpace(venta.RazonSocial)
                    ? venta.Cliente
                    : venta.RazonSocial,
                Cajero = await ObtenerNombreUsuarioAsync(venta.IdUsuario),
                MetodoPago = venta.MetodoPago,
                Estado = venta.Estado,
                EstadoSaga = venta.EstadoSaga,
                Total = venta.Total
            };

            foreach (DetalleVentaReporteDto detalle in venta.Detalles)
            {
                comprobante.Detalles.Add(new ComprobanteVentaDetalleDto
                {
                    IdMedicamento = detalle.IdMedicamento,
                    Medicamento = await ObtenerNombreMedicamentoAsync(detalle.IdMedicamento),
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal = detalle.Subtotal
                });
            }

            return comprobante;
        }

        private async Task<string> ObtenerNombreMedicamentoAsync(int idMedicamento)
        {
            try
            {
                HttpClient productosClient = CrearClienteAutenticado("MSProductos");
                HttpResponseMessage response = await productosClient.GetAsync($"api/medicamentos/{idMedicamento}");

                if (!response.IsSuccessStatusCode)
                    return $"Medicamento #{idMedicamento}";

                MedicamentoReporteDto? medicamento =
                    await response.Content.ReadFromJsonAsync<MedicamentoReporteDto>(JsonOptions);

                if (medicamento == null)
                    return $"Medicamento #{idMedicamento}";

                string descripcion = string.Join(
                    " - ",
                    new[] { medicamento.Nombre, medicamento.Presentacion }
                        .Where(valor => !string.IsNullOrWhiteSpace(valor)));

                return string.IsNullOrWhiteSpace(descripcion)
                    ? $"Medicamento #{idMedicamento}"
                    : descripcion;
            }
            catch
            {
                return $"Medicamento #{idMedicamento}";
            }
        }

        private async Task<List<VentaReporteDto>> ObtenerVentasConfirmadasAsync(
            DateTime? desde,
            DateTime? hasta)
        {
            HttpClient ventasClient = CrearClienteAutenticado("MSVentas");
            HttpResponseMessage response = await ventasClient.GetAsync("api/ventas");

            response.EnsureSuccessStatusCode();

            RespuestaVentasDto? respuestaVentas =
                await response.Content.ReadFromJsonAsync<RespuestaVentasDto>(JsonOptions);

            IEnumerable<VentaReporteDto> ventas = respuestaVentas?.Data ?? new List<VentaReporteDto>();
            DateTime? desdeNormalizado = desde?.Date;
            DateTime? hastaExclusivo = hasta?.Date.AddDays(1);

            return ventas
                .Where(venta => string.Equals(venta.Estado, "ACTIVA", StringComparison.OrdinalIgnoreCase))
                .Where(venta => string.Equals(venta.EstadoSaga, "STOCK_CONFIRMADO", StringComparison.OrdinalIgnoreCase))
                .Where(venta => !desdeNormalizado.HasValue || ObtenerFechaVenta(venta) >= desdeNormalizado.Value)
                .Where(venta => !hastaExclusivo.HasValue || ObtenerFechaVenta(venta) < hastaExclusivo.Value)
                .ToList();
        }

        private async Task<string> ObtenerRolUsuarioAsync(int idUsuario)
        {
            if (idUsuario <= 0)
                return "Rol no identificado";

            try
            {
                HttpClient usuariosClient = CrearClienteAutenticado("MSUsuarios");
                HttpResponseMessage response =
                    await usuariosClient.GetAsync($"api/usuarios/getUserById?id={idUsuario}");

                if (!response.IsSuccessStatusCode)
                    return "Rol no identificado";

                RespuestaUsuarioDto? respuestaUsuario =
                    await response.Content.ReadFromJsonAsync<RespuestaUsuarioDto>(JsonOptions);

                string rol = NormalizarRol(respuestaUsuario?.Data?.Role);
                return string.IsNullOrWhiteSpace(rol) ? "Rol no identificado" : rol;
            }
            catch
            {
                return "Rol no identificado";
            }
        }

        private async Task<string> ObtenerNombreUsuarioAsync(int idUsuario)
        {
            if (idUsuario <= 0)
                return "Usuario no identificado";

            try
            {
                HttpClient usuariosClient = CrearClienteAutenticado("MSUsuarios");
                HttpResponseMessage response =
                    await usuariosClient.GetAsync($"api/usuarios/getUserById?id={idUsuario}");

                if (!response.IsSuccessStatusCode)
                    return $"Usuario #{idUsuario}";

                RespuestaUsuarioDto? respuestaUsuario =
                    await response.Content.ReadFromJsonAsync<RespuestaUsuarioDto>(JsonOptions);

                UsuarioReporteDto? usuario = respuestaUsuario?.Data;
                if (usuario == null)
                    return $"Usuario #{idUsuario}";

                string nombre = string.Join(
                    " ",
                    new[] { usuario.Nombres, usuario.ApellidoPaterno, usuario.ApellidoMaterno }
                        .Where(valor => !string.IsNullOrWhiteSpace(valor)));

                if (string.IsNullOrWhiteSpace(nombre))
                    nombre = string.IsNullOrWhiteSpace(usuario.UserName)
                        ? $"Usuario #{idUsuario}"
                        : usuario.UserName;

                return $"{nombre} (ID: {idUsuario})";
            }
            catch
            {
                return $"Usuario #{idUsuario}";
            }
        }

        private HttpClient CrearClienteAutenticado(string nombreCliente)
        {
            HttpClient client = httpClientFactory.CreateClient(nombreCliente);
            string? authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization;

            if (!string.IsNullOrWhiteSpace(authorization) &&
                AuthenticationHeaderValue.TryParse(authorization, out AuthenticationHeaderValue? header))
            {
                client.DefaultRequestHeaders.Authorization = header;
            }

            return client;
        }

        private class RespuestaVentaDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public VentaReporteDto? Data { get; set; }
        }

        private class RespuestaVentasDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public List<VentaReporteDto> Data { get; set; } = new();
        }

        private class VentaReporteDto
        {
            public int Id { get; set; }
            public DateTime Fecha { get; set; }
            public DateTime FechaHora { get; set; }
            public string Nit { get; set; } = string.Empty;
            public string RazonSocial { get; set; } = string.Empty;
            public string Cliente { get; set; } = string.Empty;
            public int IdUsuario { get; set; }
            public string Usuario { get; set; } = string.Empty;
            public string MetodoPago { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public string Estado { get; set; } = string.Empty;
            public string EstadoSaga { get; set; } = string.Empty;
            public List<DetalleVentaReporteDto> Detalles { get; set; } = new();
        }

        private class DetalleVentaReporteDto
        {
            public int IdMedicamento { get; set; }
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
            public decimal Subtotal { get; set; }
        }

        private class MedicamentoReporteDto
        {
            public string Nombre { get; set; } = string.Empty;
            public string Presentacion { get; set; } = string.Empty;
        }

        private class RespuestaUsuarioDto
        {
            public string Mensaje { get; set; } = string.Empty;
            public UsuarioReporteDto? Data { get; set; }
        }

        private class UsuarioReporteDto
        {
            public string Nombres { get; set; } = string.Empty;
            public string ApellidoPaterno { get; set; } = string.Empty;
            public string? ApellidoMaterno { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        private static DateTime ObtenerFechaVenta(VentaReporteDto venta)
        {
            return venta.FechaHora != default ? venta.FechaHora : venta.Fecha;
        }

        private static string NormalizarRol(string? rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                return string.Empty;

            string limpio = rol.Trim();
            string normalizado = limpio
                .Replace("í", "i")
                .Replace("Í", "I")
                .ToUpperInvariant();

            return normalizado switch
            {
                "ADMIN" or "ADMINISTRADOR" => "Admin",
                "BIOQUIMICO" => "Bioquimico",
                _ => limpio
            };
        }
    }
}
