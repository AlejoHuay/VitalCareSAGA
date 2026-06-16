using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MSReportes.API.FrameworksYDrivers.Creadores
{
    public class ReporteVentasPorRolPdfCreador : IReporteVentasPdfCreador
    {
        public ArchivoReporteDto Crear(ReporteVentasPorRol reporte)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            byte[]? logoBytes = ObtenerLogo();
            ReporteVentasPorRolDto? rolMasVentas = ObtenerRolMasVentas(reporte);
            ReporteVentasPorRolDto? rolMasRecaudacion = ObtenerRolMasRecaudacion(reporte);
            decimal maximoRecaudado = reporte.Detalle.Count > 0
                ? reporte.Detalle.Max(item => item.TotalRecaudado)
                : 0;

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
                            row.ConstantItem(72).Height(72).Element(logo =>
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

                                header.Item().Text(reporte.Titulo)
                                    .FontSize(15)
                                    .Bold()
                                    .FontColor(Colors.Teal.Darken2);

                                header.Item().Text($"Periodo: {reporte.PeriodoTexto}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);

                                header.Item().Text($"Fecha de generacion: {reporte.FechaGeneracion:dd/MM/yyyy HH:mm:ss}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                        });

                        content.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        content.Item().Row(row =>
                        {
                            AgregarResumen(row.RelativeItem(), "Cantidad de ventas", reporte.Resumen.TotalVentas.ToString(), "Operaciones confirmadas");
                            row.ConstantItem(8);
                            AgregarResumen(row.RelativeItem(), "Total vendido", $"Bs {reporte.Resumen.TotalRecaudado:N2}", "Importe acumulado");
                            row.ConstantItem(8);
                            AgregarResumen(row.RelativeItem(), "Rol con mas ventas", rolMasVentas?.Rol ?? "-", $"{rolMasVentas?.CantidadVentas ?? 0} ventas");
                            row.ConstantItem(8);
                            AgregarResumen(row.RelativeItem(), "Mayor recaudacion", rolMasRecaudacion?.Rol ?? "-", $"Bs {rolMasRecaudacion?.TotalRecaudado ?? 0:N2}");
                        });

                        content.Item().Column(section =>
                        {
                            section.Spacing(8);
                            section.Item().Text("Visualizacion por recaudacion")
                                .FontSize(12)
                                .Bold()
                                .FontColor(Colors.Green.Darken2);

                            if (reporte.Detalle.Count == 0)
                            {
                                section.Item().Border(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Padding(12)
                                    .AlignCenter()
                                    .Text("No hay datos para graficar en el periodo seleccionado.")
                                    .FontColor(Colors.Grey.Darken1);
                            }
                            else
                            {
                                foreach (ReporteVentasPorRolDto item in reporte.Detalle)
                                {
                                    decimal proporcion = maximoRecaudado > 0
                                        ? item.TotalRecaudado / maximoRecaudado
                                        : 0;

                                    section.Item().Row(row =>
                                    {
                                        row.ConstantItem(105).Text(item.Rol).SemiBold();

                                        row.RelativeItem().Height(12).Background(Colors.Green.Lighten5).Element(bar =>
                                        {
                                            bar.Width(Math.Max(20, (float)(proporcion * 260)))
                                                .Height(12)
                                                .Background(Colors.Green.Darken1);
                                        });

                                        row.ConstantItem(90).AlignRight().Text($"Bs {item.TotalRecaudado:N2}").SemiBold();
                                    });
                                }
                            }
                        });

                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(110);
                                columns.ConstantColumn(130);
                                columns.ConstantColumn(90);
                            });

                            table.Header(header =>
                            {
                                AgregarCeldaEncabezado(header.Cell(), "Rol / Usuario");
                                AgregarCeldaEncabezado(header.Cell(), "Ventas");
                                AgregarCeldaEncabezado(header.Cell(), "Total vendido");
                                AgregarCeldaEncabezado(header.Cell(), "Participacion");
                            });

                            foreach (ReporteVentasPorRolDto item in reporte.Detalle)
                            {
                                decimal participacion = reporte.Resumen.TotalRecaudado > 0
                                    ? item.TotalRecaudado / reporte.Resumen.TotalRecaudado * 100
                                    : 0;

                                AgregarCeldaGrupo(table.Cell(), item.Rol);
                                AgregarCeldaGrupoCentrada(table.Cell(), item.CantidadVentas.ToString());
                                AgregarCeldaGrupoDerecha(table.Cell(), $"Bs {item.TotalRecaudado:N2}");
                                AgregarCeldaGrupoDerecha(table.Cell(), $"{participacion:N1}%");

                                foreach (ReporteVentasPorUsuarioDto usuario in item.Usuarios)
                                {
                                    decimal participacionUsuario = item.TotalRecaudado > 0
                                        ? usuario.TotalRecaudado / item.TotalRecaudado * 100
                                        : 0;

                                    AgregarCeldaDetalle(table.Cell(), usuario.NombreUsuario);
                                    AgregarCeldaDetalleCentrada(table.Cell(), usuario.CantidadVentas.ToString());
                                    AgregarCeldaDetalleDerecha(table.Cell(), $"Bs {usuario.TotalRecaudado:N2}");
                                    AgregarCeldaDetalleDerecha(table.Cell(), $"{participacionUsuario:N1}% del rol");
                                }
                            }

                            AgregarCeldaTotal(table.Cell(), "TOTAL");
                            AgregarCeldaTotalCentrada(table.Cell(), reporte.Resumen.TotalVentas.ToString());
                            AgregarCeldaTotalDerecha(table.Cell(), $"Bs {reporte.Resumen.TotalRecaudado:N2}");
                            AgregarCeldaTotalDerecha(table.Cell(), "100%");
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span(reporte.PiePagina).FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.Span(" | Pagina ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return new ArchivoReporteDto
            {
                NombreArchivo = $"reporte-ventas-por-rol-{DateTime.Now:yyyyMMddHHmmss}.pdf",
                ContentType = "application/pdf",
                Contenido = contenido
            };
        }

        private static void AgregarResumen(IContainer container, string titulo, string valor, string detalle)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten5)
                .Padding(8)
                .Column(column =>
                {
                    column.Spacing(3);
                    column.Item().Text(titulo).FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().Text(valor).FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                    column.Item().Text(detalle).FontSize(8).FontColor(Colors.Grey.Darken1);
                });
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

        private static void AgregarCeldaGrupo(IContainer container, string texto)
        {
            container.Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(texto).Bold();
        }

        private static void AgregarCeldaGrupoCentrada(IContainer container, string texto)
        {
            container.Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(texto).Bold();
        }

        private static void AgregarCeldaGrupoDerecha(IContainer container, string texto)
        {
            container.Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(texto).Bold();
        }

        private static void AgregarCeldaDetalle(IContainer container, string texto)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).PaddingLeft(16).Text(texto);
        }

        private static void AgregarCeldaDetalleCentrada(IContainer container, string texto)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(texto);
        }

        private static void AgregarCeldaDetalleDerecha(IContainer container, string texto)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(texto);
        }

        private static void AgregarCeldaTotal(IContainer container, string texto)
        {
            container.Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(texto).Bold();
        }

        private static void AgregarCeldaTotalCentrada(IContainer container, string texto)
        {
            container.Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(texto).Bold();
        }

        private static void AgregarCeldaTotalDerecha(IContainer container, string texto)
        {
            container.Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignRight().Text(texto).Bold();
        }

        private static ReporteVentasPorRolDto? ObtenerRolMasVentas(ReporteVentasPorRol reporte)
        {
            return reporte.Detalle
                .OrderByDescending(item => item.CantidadVentas)
                .ThenByDescending(item => item.TotalRecaudado)
                .FirstOrDefault();
        }

        private static ReporteVentasPorRolDto? ObtenerRolMasRecaudacion(ReporteVentasPorRol reporte)
        {
            return reporte.Detalle
                .OrderByDescending(item => item.TotalRecaudado)
                .ThenByDescending(item => item.CantidadVentas)
                .FirstOrDefault();
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
