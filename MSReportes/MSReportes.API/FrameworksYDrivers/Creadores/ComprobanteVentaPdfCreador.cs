using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.Entidades;
using MSReportes.API.Helpers;
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
            byte[]? logoBytes = ObtenerLogo();

            byte[] contenido = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(text => text.FontSize(10).FontColor(Colors.Grey.Darken4));

                    page.Content().Column(content =>
                    {
                        content.Spacing(14);

                        content.Item().Row(row =>
                        {
                            row.ConstantItem(82).Height(82).Element(logo =>
                            {
                                if (logoBytes != null)
                                {
                                    logo.Image(logoBytes).FitArea();
                                }
                                else
                                {
                                    logo.Border(1)
                                        .BorderColor(Colors.Grey.Lighten2)
                                        .AlignCenter()
                                        .AlignMiddle()
                                        .Text("VitalCare")
                                        .SemiBold()
                                        .FontColor(Colors.Green.Darken2);
                                }
                            });

                            row.RelativeItem().PaddingLeft(12).Column(header =>
                            {
                                header.Spacing(3);

                                header.Item().Text("Farmacia VitalCare")
                                    .FontSize(20)
                                    .Bold()
                                    .FontColor(Colors.Green.Darken2);

                                header.Item().Text("COMPROBANTE DE VENTA")
                                    .FontSize(15)
                                    .Bold()
                                    .FontColor(Colors.Teal.Darken2);

                                header.Item().Text($"Fecha: {comprobante.Fecha:dd/MM/yyyy HH:mm:ss}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);

                                header.Item().Text($"Nro. venta: {comprobante.IdVenta}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });

                        content.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        content.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(info =>
                        {
                            info.Spacing(8);

                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("CI / NIT").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.Nit).SemiBold();
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Razon social").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.RazonSocial).SemiBold();
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Cajero").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.Cajero).SemiBold();
                                });
                            });

                            info.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("Metodo de pago").FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(comprobante.MetodoPago).SemiBold();
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
                                AgregarCeldaCentrada(table.Cell(), detalle.Cantidad.ToString());
                                AgregarCelda(table.Cell(), detalle.Medicamento);
                                AgregarCeldaDerecha(table.Cell(), $"{detalle.PrecioUnitario:0.00}");
                                AgregarCeldaDerecha(table.Cell(), $"{detalle.Subtotal:0.00}");
                            }
                        });

                        int centavos = ObtenerCentavos(comprobante.Total);
                        string totalLiteral =
                            NumeroATextoConverter.ConvertirDecimalATexto(comprobante.Total);

                        content.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Son {totalLiteral} {centavos:00}/100 Bolivianos")
                                .FontSize(11)
                                .FontColor(Colors.Black);

                            row.ConstantItem(210).AlignRight().Text($"TOTAL Bs.: {comprobante.Total:0.00}")
                                .FontSize(15)
                                .Bold()
                                .FontColor(Colors.Green.Darken2);
                        });

                        content.Item().PaddingTop(8).AlignCenter().Text("Gracias por su compra.")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Documento generado por VitalCare").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
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
                .AlignCenter()
                .Text(texto)
                .SemiBold();
        }

        private static void AgregarCelda(IContainer container, string texto)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .Text(texto);
        }

        private static void AgregarCeldaCentrada(IContainer container, string texto)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .AlignCenter()
                .Text(texto);
        }

        private static void AgregarCeldaDerecha(IContainer container, string texto)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .AlignRight()
                .Text(texto);
        }

        private static int ObtenerCentavos(decimal total)
        {
            decimal parteDecimal = total - Math.Truncate(total);
            return (int)Math.Round(parteDecimal * 100, 0, MidpointRounding.AwayFromZero);
        }

        private static byte[]? ObtenerLogo()
        {
            string logoPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "vitalcare.png");

            return File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : null;
        }
    }
}
