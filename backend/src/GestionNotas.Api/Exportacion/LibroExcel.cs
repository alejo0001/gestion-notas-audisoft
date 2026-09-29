using ClosedXML.Excel;

namespace GestionNotas.Api.Exportacion;

/// <summary>Una columna del Excel: título, cómo obtener el valor de cada fila, ancho y formato numérico.</summary>
public sealed record ColumnaExcel<T>(string Titulo, Func<T, object?> Valor, double Ancho = 15, string? Formato = null);

/// <summary>
/// Genera un .xlsx a partir de una lista de filas y su definición de columnas (ClosedXML).
/// Genérico: los tres recursos lo reutilizan; cada controlador solo declara sus columnas.
/// Vive en la capa del API porque el Excel es una representación más del recurso, como el JSON:
/// los servicios de Application devuelven datos y no saben nada de formatos de archivo.
/// </summary>
public static class LibroExcel
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static byte[] Crear<T>(string nombreHoja, IReadOnlyList<ColumnaExcel<T>> columnas, IReadOnlyList<T> filas)
    {
        using var libro = new XLWorkbook();
        libro.Properties.Title = $"Gestión de Notas — {nombreHoja}";
        libro.Properties.Author = "Gestión de Notas";

        var hoja = libro.Worksheets.Add(nombreHoja);

        // Encabezado.
        for (var c = 0; c < columnas.Count; c++)
        {
            var celda = hoja.Cell(1, c + 1);
            celda.Value = columnas[c].Titulo;
            hoja.Column(c + 1).Width = columnas[c].Ancho;
        }

        var encabezado = hoja.Range(1, 1, 1, columnas.Count);
        encabezado.Style.Font.Bold = true;
        encabezado.Style.Font.FontColor = XLColor.White;
        encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#1565C0");

        // Filas. XLCellValue.FromObject conserva el tipo: los números quedan como números (se pueden
        // sumar y filtrar en Excel), no como texto.
        for (var f = 0; f < filas.Count; f++)
        {
            for (var c = 0; c < columnas.Count; c++)
            {
                var celda = hoja.Cell(f + 2, c + 1);
                celda.Value = XLCellValue.FromObject(columnas[c].Valor(filas[f]));
                if (columnas[c].Formato is { } formato)
                {
                    celda.Style.NumberFormat.Format = formato;
                }
            }
        }

        // Filtros en el encabezado y encabezado fijo al desplazarse.
        hoja.Range(1, 1, Math.Max(1, filas.Count + 1), columnas.Count).SetAutoFilter();
        hoja.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        libro.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>Nombre de archivo con fecha, p. ej. «notas-2026-09-29.xlsx».</summary>
    public static string NombreArchivo(string recurso) => $"{recurso}-{DateTime.UtcNow:yyyy-MM-dd}.xlsx";
}
