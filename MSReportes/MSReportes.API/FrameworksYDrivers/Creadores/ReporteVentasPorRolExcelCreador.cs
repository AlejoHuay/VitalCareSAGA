using ClosedXML.Excel;
using MSReportes.API.AdaptadoresDeInterfaz.Gateways;
using MSReportes.API.Entidades;

namespace MSReportes.API.FrameworksYDrivers.Creadores
{
    public class ReporteVentasPorRolExcelCreador : IReporteVentasExcelCreador
    {
        public ArchivoReporteDto Crear(ReporteVentasPorRol reporte)
        {
            using var workbook = new XLWorkbook();

            CrearHojaResumen(workbook, reporte);
            CrearHojaDetalle(workbook, reporte);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return new ArchivoReporteDto
            {
                NombreArchivo = $"reporte-ventas-por-rol-{DateTime.Now:yyyyMMddHHmmss}.xlsx",
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Contenido = stream.ToArray()
            };
        }

        private static void CrearHojaResumen(XLWorkbook workbook, ReporteVentasPorRol reporte)
        {
            var worksheet = workbook.Worksheets.Add("Resumen");
            worksheet.Style.Font.FontName = "Calibri";

            worksheet.Cell("A1").Value = "Farmacia VitalCare";
            worksheet.Cell("A2").Value = reporte.Titulo;
            worksheet.Cell("A3").Value = $"Periodo: {reporte.PeriodoTexto}";
            worksheet.Cell("A4").Value = $"Generado: {reporte.FechaGeneracion:dd/MM/yyyy HH:mm:ss}";

            worksheet.Range("A1:D1").Merge().Style.Font.SetBold().Font.SetFontSize(18).Font.FontColor = XLColor.FromHtml("#15803D");
            worksheet.Range("A2:D2").Merge().Style.Font.SetBold().Font.SetFontSize(14).Font.FontColor = XLColor.FromHtml("#0F766E");
            worksheet.Range("A3:D4").Style.Font.FontColor = XLColor.FromHtml("#64748B");

            AgregarTarjetaResumen(worksheet, "A6", "Cantidad de ventas", reporte.Resumen.TotalVentas.ToString());
            AgregarTarjetaResumen(worksheet, "C6", "Total vendido", $"Bs {reporte.Resumen.TotalRecaudado:N2}");

            ReporteVentasPorRolDto? rolMasVentas = ObtenerRolMasVentas(reporte);
            ReporteVentasPorRolDto? rolMasRecaudacion = ObtenerRolMasRecaudacion(reporte);

            AgregarTarjetaResumen(worksheet, "A10", "Rol con mas ventas", rolMasVentas?.Rol ?? "-");
            AgregarTarjetaResumen(worksheet, "C10", "Mayor recaudacion", rolMasRecaudacion?.Rol ?? "-");

            worksheet.Cell("A15").Value = "Visualizacion por recaudacion";
            worksheet.Cell("A15").Style.Font.SetBold().Font.FontColor = XLColor.FromHtml("#15803D");

            worksheet.Cell("A17").Value = "Rol";
            worksheet.Cell("B17").Value = "Total vendido";
            worksheet.Cell("C17").Value = "Participacion";
            worksheet.Cell("D17").Value = "Referencia";
            AplicarEncabezado(worksheet.Range("A17:D17"));

            decimal maximo = reporte.Detalle.Count > 0 ? reporte.Detalle.Max(item => item.TotalRecaudado) : 0;
            int fila = 18;

            foreach (ReporteVentasPorRolDto item in reporte.Detalle)
            {
                decimal participacion = reporte.Resumen.TotalRecaudado > 0
                    ? item.TotalRecaudado / reporte.Resumen.TotalRecaudado
                    : 0;

                decimal referencia = maximo > 0 ? item.TotalRecaudado / maximo : 0;

                worksheet.Cell(fila, 1).Value = item.Rol;
                worksheet.Cell(fila, 2).Value = item.TotalRecaudado;
                worksheet.Cell(fila, 3).Value = participacion;
                worksheet.Cell(fila, 4).Value = referencia;

                worksheet.Cell(fila, 2).Style.NumberFormat.Format = "\"Bs\" #,##0.00";
                worksheet.Cell(fila, 3).Style.NumberFormat.Format = "0.0%";
                worksheet.Cell(fila, 4).Style.NumberFormat.Format = "0%";
                fila++;
            }

            if (fila > 18)
            {
                worksheet.Range(18, 4, fila - 1, 4)
                    .AddConditionalFormat()
                    .DataBar(XLColor.FromHtml("#16A34A"));
            }
            else
            {
                worksheet.Cell("A18").Value = "No hay datos para mostrar.";
                worksheet.Range("A18:D18").Merge().Style.Font.FontColor = XLColor.FromHtml("#64748B");
            }

            worksheet.Columns("A:D").AdjustToContents();
            worksheet.Column("D").Width = 28;
        }

        private static void CrearHojaDetalle(XLWorkbook workbook, ReporteVentasPorRol reporte)
        {
            var worksheet = workbook.Worksheets.Add("Detalle");
            worksheet.Style.Font.FontName = "Calibri";

            worksheet.Cell("A1").Value = reporte.Titulo;
            worksheet.Cell("A2").Value = reporte.PeriodoTexto;
            worksheet.Range("A1:D1").Merge().Style.Font.SetBold().Font.SetFontSize(16).Font.FontColor = XLColor.FromHtml("#15803D");
            worksheet.Range("A2:D2").Merge().Style.Font.FontColor = XLColor.FromHtml("#64748B");

            worksheet.Cell("A4").Value = "Rol";
            worksheet.Cell("B4").Value = "Usuario";
            worksheet.Cell("C4").Value = "Cantidad de ventas";
            worksheet.Cell("D4").Value = "Total vendido (Bs)";
            worksheet.Cell("E4").Value = "Participacion";
            AplicarEncabezado(worksheet.Range("A4:E4"));

            int fila = 5;

            foreach (ReporteVentasPorRolDto item in reporte.Detalle)
            {
                decimal participacionRol = reporte.Resumen.TotalRecaudado > 0
                    ? item.TotalRecaudado / reporte.Resumen.TotalRecaudado
                    : 0;

                worksheet.Cell(fila, 1).Value = item.Rol;
                worksheet.Cell(fila, 2).Value = "Total del rol";
                worksheet.Cell(fila, 3).Value = item.CantidadVentas;
                worksheet.Cell(fila, 4).Value = item.TotalRecaudado;
                worksheet.Cell(fila, 5).Value = participacionRol;
                worksheet.Range(fila, 1, fila, 5).Style.Font.SetBold();
                worksheet.Range(fila, 1, fila, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
                worksheet.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(fila, 5).Style.NumberFormat.Format = "0.0%";
                fila++;

                foreach (ReporteVentasPorUsuarioDto usuario in item.Usuarios)
                {
                    decimal participacionUsuario = item.TotalRecaudado > 0
                        ? usuario.TotalRecaudado / item.TotalRecaudado
                        : 0;

                    worksheet.Cell(fila, 1).Value = item.Rol;
                    worksheet.Cell(fila, 2).Value = usuario.NombreUsuario;
                    worksheet.Cell(fila, 3).Value = usuario.CantidadVentas;
                    worksheet.Cell(fila, 4).Value = usuario.TotalRecaudado;
                    worksheet.Cell(fila, 5).Value = participacionUsuario;
                    worksheet.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(fila, 5).Style.NumberFormat.Format = "0.0%";
                    fila++;
                }
            }

            worksheet.Cell(fila, 1).Value = "TOTAL";
            worksheet.Cell(fila, 2).Value = "Todos los roles";
            worksheet.Cell(fila, 3).Value = reporte.Resumen.TotalVentas;
            worksheet.Cell(fila, 4).Value = reporte.Resumen.TotalRecaudado;
            worksheet.Cell(fila, 5).Value = reporte.Resumen.TotalRecaudado > 0 ? 1 : 0;
            worksheet.Range(fila, 1, fila, 5).Style.Font.SetBold();
            worksheet.Range(fila, 1, fila, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F5F9");
            worksheet.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(fila, 5).Style.NumberFormat.Format = "0.0%";

            var usedRange = worksheet.Range(4, 1, fila, 5);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
            usedRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#CBD5E1");

            worksheet.Columns("A:E").AdjustToContents();
        }

        private static void AgregarTarjetaResumen(IXLWorksheet worksheet, string celda, string titulo, string valor)
        {
            var cell = worksheet.Cell(celda);
            var rango = worksheet.Range(cell, cell.CellRight());
            rango.Merge();
            rango.Style.Fill.BackgroundColor = XLColor.FromHtml("#F8FAFC");
            rango.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rango.Style.Border.OutsideBorderColor = XLColor.FromHtml("#CBD5E1");
            rango.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            cell.Value = $"{titulo}{Environment.NewLine}{valor}";
            cell.Style.Alignment.WrapText = true;
            cell.Style.Font.FontColor = XLColor.FromHtml("#0F172A");
            cell.Style.Font.SetBold();
            worksheet.Row(cell.Address.RowNumber).Height = 45;
        }

        private static void AplicarEncabezado(IXLRange range)
        {
            range.Style.Font.SetBold();
            range.Style.Fill.BackgroundColor = XLColor.FromHtml("#D1E7DD");
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = XLColor.FromHtml("#A7C7B7");
            range.Style.Border.InsideBorderColor = XLColor.FromHtml("#A7C7B7");
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
    }
}
