using Microsoft.EntityFrameworkCore;

namespace GestionNotas.Application.Common;

/// <summary>Parámetros de consulta comunes para listados paginados (se enlazan desde el query string).</summary>
public record PagedQuery
{
    public const int MaxPageSize = 100;

    /// <summary>Tope de filas al exportar: evita generar archivos enormes por error o por abuso.</summary>
    public const int MaxFilasExportacion = 10_000;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; } = "asc";

    public bool IsDescending =>
        string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

    public int NormalizedPage => Math.Max(1, Page);

    public int NormalizedPageSize => Math.Clamp(PageSize, 1, MaxPageSize);

    public string? NormalizedSearch => string.IsNullOrWhiteSpace(Search) ? null : Search.Trim();
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public static class PagingExtensions
{
    /// <summary>
    /// Ejecuta el conteo total y la página solicitada (Skip/Take) directamente en la base de datos.
    /// La consulta debe venir ya filtrada y ordenada: Skip/Take sin ORDER BY no es determinista.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PagedQuery paging,
        CancellationToken cancellationToken)
    {
        var page = paging.NormalizedPage;
        var pageSize = paging.NormalizedPageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
