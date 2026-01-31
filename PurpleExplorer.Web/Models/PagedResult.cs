using System.Collections.Generic;

namespace PurpleExplorer.Web.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public string? ContinuationToken { get; set; }
}
