using System.Collections.Generic;

namespace PurpleExplorer.Core.Models;

public class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, string? continuationToken)
    {
        Items = items;
        ContinuationToken = continuationToken;
    }

    public IReadOnlyList<T> Items { get; }
    public string? ContinuationToken { get; }
}
