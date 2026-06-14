using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MSReportes.API.FrameworksYDrivers.Creadores
{
    public class ComprobanteVentaPdfCreador : IComprobanteVentaPdfCreador
    {
        public ArchivoReporteDto Crear(ComprobanteVentaDto comprobante)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            byte[] contenido = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(text => text.FontSize(10));

                    page.Header().Column(header =>
                    {
                        header.Item().Text("Farmacia VitalCare")
                            .FontSize(20)
                            .SemiBold()
                            .FontColor(Colors.Green.Darken2);

                        header.Item().Text("COMPROBANTE DE VENTA")
                            .FontSize(18)
                            .SemiBold()
                            .FontColor(Colors.Black);

                        header.Item().Text($"Nro. venta: {comprobante.IdVenta}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(25).Column(content =>
                    {
                        content.Spacing(18);

                        content.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(info =>
                        {
                            info.Spacing(8);

                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Fecha").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.Fecha.ToString("dd/MM/yyyy HH:mm")).SemiBold();
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("NIT / CI").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.Nit).SemiBold();
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Metodo de pago").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.MetodoPago).SemiBold();
                                });
                            });

                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Cliente").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.RazonSocial).SemiBold();
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Cajero").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.Cajero).SemiBold();
                                });
                            });
                        });

                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn();
                                columns.ConstantColumn(90);
                                columns.ConstantColumn(90);
                            });

                            table.Header(header =>
                            {
                                AgregarCeldaEncabezado(header.Cell(), "Cant.");
                                AgregarCeldaEncabezado(header.Cell(), "Descripcion");
                                AgregarCeldaEncabezado(header.Cell(), "P. Unit.");
                                AgregarCeldaEncabezado(header.Cell(), "Importe");
                            });

                            foreach (ComprobanteVentaDetalleDto detalle in comprobante.Detalles)
                            {
                                AgregarCelda(table.Cell(), detalle.Cantidad.ToString());
                                AgregarCelda(table.Cell(), detalle.Medicamento);
                                AgregarCelda(table.Cell(), $"Bs {detalle.PrecioUnitario:N2}");
                                AgregarCelda(table.Cell(), $"Bs {detalle.Subtotal:N2}");
                            }
                        });

                        content.Item().AlignRight().Column(total =>
                        {
                            total.Item().Text($"Total: Bs {comprobante.Total:N2}")
                                .FontSize(16)
                                .SemiBold()
                                .FontColor(Colors.Green.Darken2);

                            total.Item().Text($"Son: {comprobante.Total:N2} bolivianos")
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });

                    page.Footer().AlignCenter().Text("Gracias por su compra.");
                });
            }).GeneratePdf();

            return new ArchivoReporteDto
            {
                NombreArchivo = $"comprobante-venta-{comprobante.IdVenta}-{DateTime.Now:yyyyMMddHHmmss}.pdf",
                ContentType = "application/pdf",
                Contenido = contenido
            };
        }

        private static void AgregarCeldaEncabezado(IContainer container, string texto)
        {
            container
                .Background(Colors.Green.Lighten4)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .Text(texto)
                .SemiBold();
        }

        private static void AgregarCelda(IContainer container, string texto)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .Text(texto);
        }
    }
}
