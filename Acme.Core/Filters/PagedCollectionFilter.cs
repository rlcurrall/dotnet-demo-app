using Microsoft.AspNetCore.Mvc;

namespace Acme.Core.Filters;

public class PagedCollectionFilter
{
    /// <summary>
    ///     Page to fetch (optional).
    /// </summary>
    [ModelBinder(Name = "page")]
    public int? Page { get; set; }

    /// <summary>
    ///     Amount of items to fetch (optional).
    /// </summary>
    [ModelBinder(Name = "pageSize")]
    public int? PageSize { get; set; }

    /// <summary>
    ///     Sort by KEY (entity field name) (optional).
    /// </summary>
    [ModelBinder(Name = "sortBy")]
    public string SortBy { get; set; }

    /// <summary>
    ///     Sort direction (optional).
    ///     Allowed: "ascending" / "asc" / "descending" / "desc".
    /// </summary>
    [ModelBinder(Name = "sortOperator")]
    public string SortOperator { get; set; }

    /// <summary>
    ///     Include deleted entities (optional)?
    /// </summary>
    [ModelBinder(Name = "includeDeleted")]
    public bool? IncludeDeleted { get; set; }
}