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

        public async Task<IEnumerable<ReporteVentasPorRolDto>> ObtenerVentasPorRolAsync()
        {
            await Task.CompletedTask;

            return new List<ReporteVentasPorRolDto>
            {
                new ReporteVentasPorRolDto
                {
                    Rol = "Administrador",
                    CantidadVentas = 10,
                    TotalRecaudado = 1500
                },
                new ReporteVentasPorRolDto
                {
                    Rol = "Vendedor",
                    CantidadVentas = 25,
                    TotalRecaudado = 3800
                }
            };
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
                Cajero = string.IsNullOrWhiteSpace(venta.Usuario)
                    ? $"Usuario #{venta.IdUsuario}"
                    : venta.Usuario,
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
    }
}
